var tokenKey = "accessToken";

async function updateCameras()
{
    const token = sessionStorage.getItem(tokenKey);
    const userId = sessionStorage.getItem("userId");

    const response = await fetch("/cameras/" + userId, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token  // передача токена в заголовке
        },
    });
    if (response.ok === true) {
        const data = await response.json();
        const cameras = data.cameras;

        const container = document.getElementById("camerasContainer");
        container.innerHTML = "";
        for (let i = 0; i < cameras.length; i++) {
            const cameraName = cameras[i].name;
            const hr = document.createElement("hr");
            container.appendChild(hr);
            const cameraInfo = document.createElement("div");
            const p = document.createElement("p");

            

            p.innerHTML += `Camera ${cameraName}  `;
            
            p.innerHTML += "<br>";
            const todayDate = new Date();
            const defaultValue = `${todayDate.getFullYear()}-${('0' + (todayDate.getMonth() + 1)).slice(-2)}-${('0' + todayDate.getDate()).slice(-2)}`;
            p.innerHTML += `Посмотреть статистику  с`;
            p.innerHTML += `<input type="date" class="fromDate" value="${defaultValue}"/> по `;
            p.innerHTML += `<input type="date" class="toDate" value="${defaultValue}"/>`;

            const visitrosChartCanvas = document.createElement("canvas");
            visitrosChartCanvas.className = "visitors-visitrosChartCanvas";
            const visitrosChart = new Chart(visitrosChartCanvas, {
                type: 'bar',
                data: {
                    labels: [],
                    datasets: [{
                        label: 'Число посетителей',
                        data: [],
                        borderWidth: 1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });

            const getCameraDataBtn = document.createElement("button");
            getCameraDataBtn.textContent = "Получить данные";
            getCameraDataBtn.addEventListener("click", (e) => drawChart(e, cameras[i].id,  visitrosChart));

            const deleteCameraBtn = document.createElement("button");
            deleteCameraBtn.textContent = "Удалить камеру";
            deleteCameraBtn.addEventListener("click", (e) => deleteCamera(e, cameras[i].id));
            

            p.appendChild(getCameraDataBtn);
            p.appendChild(deleteCameraBtn);
            p.appendChild(visitrosChartCanvas);
            cameraInfo.appendChild(p);
            container.appendChild(cameraInfo);
            
        }
        

    }
    else
        console.log("Status: ", response.status);
}

async function deleteCamera(e, cameraId)
{
    const token = sessionStorage.getItem(tokenKey);


    const response = await fetch("/deletecam/" + cameraId, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token  // передача токена в заголовке
        },
    });
    if (response.ok === true) {
        // получаем данные
        const data = await response.json();
        // изменяем содержимое и видимость блоков на странице
        alert(data.message);
        updateCameras();
    }
    else  // если произошла ошибка, получаем код статуса
        console.log("Status: ", response.status);
}
async function addCamera()
{

    const token = sessionStorage.getItem(tokenKey);
    const userId = sessionStorage.getItem("userId");
    const response = await fetch("/adddamera", {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token  // передача токена в заголовке
        },
        body: JSON.stringify({

            name: "simplename",
            address: "simpleaddress",
            userId: userId,
            fbx1: 1,
            fbx2: 2,
            fby1: 3, 
            fby2: 4,
            sbx1: 5,
            sbx2: 6,
            sby1: 7,
            sby2: 8,
        })
    });

    if (response.ok === true) {
        // получаем данные
        const data = await response.json();
        // изменяем содержимое и видимость блоков на странице
        alert(data.message);
        document.getElementById("addCameraWindow").style.display = "none";
        updateCameras();
    }
    else  // если произошла ошибка, получаем код статуса
        console.log("Status: ", response.status);

}

async function drawChart(e, cameraId, chart)
{
    const buttonParentNode = e.target.parentNode;
    const fromDate = new Date(buttonParentNode.querySelector(".fromDate").value).toLocaleDateString("en-US");
    const toDate = new Date(buttonParentNode.querySelector(".toDate").value).toLocaleDateString("en-US");
    const token = sessionStorage.getItem(tokenKey);
    const userId = sessionStorage.getItem("userId");

    //отправляем запрос к "/camera_data
    const response = await fetch("/camera_data/" + userId + "?from_date=" + fromDate + "&to_date=" + toDate, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token  // передача токена в заголовке
        },
    });

        if (response.ok === true) {
            const data = await response.json();
            const visitorsData = data.cameraValues;

            chart.data.labels = visitorsData.map(elem => elem.time);
            chart.data.datasets[0].data = visitorsData.map(elem => elem.personCount);
            chart.update();
        }
        else
            console.log("Status: ", response.status);

    
}


// при нажатии на кнопку отправки формы идет запрос к /login для получения токена
document.getElementById("submitLogin").addEventListener("click", async e => {
    e.preventDefault();
    // отправляет запрос и получаем ответ
    const response = await fetch("/login", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            login: document.getElementById("login").value,
            password: document.getElementById("password").value
        })
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const data = await response.json();
        // изменяем содержимое и видимость блоков на странице
        document.getElementById("userName").innerText = data.username;
        document.getElementById("userInfo").style.display = "block";
        document.getElementById("loginForm").style.display = "none";
        // сохраняем в хранилище sessionStorage токен доступа
        sessionStorage.setItem(tokenKey, data.access_token);
        sessionStorage.setItem("userId", data.userId);

        await updateCameras();
    }
    else  // если произошла ошибка, получаем код статуса
        console.log("Status: ", response.status);
});

// при нажатии на кнопку отправки формы идет запрос к /adduser для получения токена
document.getElementById("submitRegister").addEventListener("click", async e => {
    e.preventDefault();
    // отправляет запрос и получаем ответ
    const response = await fetch("/adduser", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            login: document.getElementById("registerlogin").value,
            email: document.getElementById("registerEmail").value,
            password: document.getElementById("registerPassword").value,
        })
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const data = await response.json();
        // изменяем содержимое и видимость блоков на странице
        document.getElementById("loginForm").style.display = "block";
        document.getElementById("registerForm").style.display = "none";
        alert(data.message);
    }
    else  // если произошла ошибка, получаем код статуса
        console.log("Status: ", response.status);
});



document.getElementById("logOut").addEventListener("click", e => {

    e.preventDefault();
    document.getElementById("userName").innerText = "";
    document.getElementById("userInfo").style.display = "none";
    document.getElementById("loginForm").style.display = "block";
    sessionStorage.removeItem(tokenKey);
});

document.getElementById("registrationBtn").addEventListener("click", e => {
    document.getElementById("loginForm").style.display = "none";
    document.getElementById("registerForm").style.display = "block";
})
document.getElementById("update").addEventListener("click", e => updateCameras());
document.getElementById("addCameraBtn").addEventListener("click", e => addCamera());
document.getElementById("openAddCameraFormBtn").addEventListener("click", e => {
    document.getElementById("addCameraWindow").style.display = "block";

});

console.log("hello");