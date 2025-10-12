// Вход пользователя
async function login()
{
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value.trim();

    if (!email || !password) {
        alert("Введите почту и пароль");
        return;
    }

    try
    {
        const response = await fetch('/User/login',
            {
                method: 'POST',
                headers:
                {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(
                    {
                        email: email,
                        password: password
                    })
            });

        // Проверяем статус ответа
        if (response.ok)
        {
            // Получаем токен из ответа
            const tokenUserId = await response.text(); // Используем text() вместо json(), так как возвращается просто строка

            // Сохраняем токен (например, в localStorage)
            localStorage.setItem('authToken', tokenUserId);

           // email.value = "";
            //password.value = "";
            document.getElementById("email").value = "";
            document.getElementById("password").value = "";
        }
        else
        {
            alert('Неверная почта или пароль');
            return;
        }
    }

    catch (error)
    {
        console.error('Ошибка:', error);
        alert('Ошибка соединения с сервером');
    }
    // переходим в чат
    window.location.href = "chats.html";
}


// Регистрация нового пользователя
const regUserBtn = document.getElementById("regUserBtn");
const regUserModal = document.getElementById("regUserModal");
const closeModal = document.getElementById("closeModal");

regUserBtn.addEventListener("click", function () {
    regUserModal.style.display = "flex";
    regUserModal.classList.add("show");
});

closeModal.addEventListener("click", () => {
    regUserModal.classList.remove("show");
    regUserModal.style.display = "none"; // закрытие по крестику

    const inputs = regUserModal.querySelectorAll("input");
    inputs.forEach(input => input.value = "");
});

const submitUserBtn = document.getElementById("submitUserBtn");
submitUserBtn.addEventListener("click", registerNewUser);

async function registerNewUser(event)
{
    event.preventDefault();

    const username = document.getElementById("username").value.trim();
    const email = document.getElementById("newEmail").value.trim();
    const password = document.getElementById("newPassword").value.trim();

    if (!username || !email || !password) {
        alert("Заполните все поля");
        return;
    }

    console.log("Регистрация:", username, email, password);

    try {
        const response = await fetch('/User/register',
            {
                method: 'POST',
                headers:
                {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(
                    {
                        username: username,
                        email: email,
                        password: password
                    })
            });

        // Проверяем статус ответа
        if (response.ok) {
            console.log("Регистрация успешна");

            regUserModal.classList.remove("show");
            regUserModal.style.display = "none";

            const inputs = regUserModal.querySelectorAll("input");
            inputs.forEach(input => input.value = "");

            Toastify({
                text: "✅ Регистрация успешна!",
                duration: 3000,
                close: true,
                gravity: "top",
                position: "right",
                backgroundColor: "#4caf50",
            }).showToast();
        }
    }
      

    catch (error)
    {
        console.error('Ошибка:', error);
        alert('Ошибка соединения с сервером');
    }
}