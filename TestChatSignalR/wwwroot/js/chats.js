
const userId = localStorage.getItem('authToken');

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/chat?userId=${userId}`)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();



//получаем сообщение на клиенте что кто-то подключился к чату
connection.on("ReceiveSystemMessage", (message, chatId) => {
    //обновляем название чата
    const currentChatName = document.getElementById("chat-title");
    currentChatId = localStorage.getItem('currentChatId');

    // Проверяем, что сообщение для текущего открытого чата
    if (chatId != currentChatId) {
        console.log("Сообщение system для другого чата, игнорируем");
        return; // Не отображаем сообщение
    }

    const msg = document.createElement("div");
    msg.textContent = message;
    msg.classList.add("message");
    msg.classList.add("system-message");
    document.getElementById("messages").appendChild(msg);

    //прокрутка диалога вниз
    const messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});

//Получаем сообщение от usera на клиенте
connection.on("ReceiveMessage", (userName, message, userId, chatId) => {
    console.log("Получаем сообщение на клиенте");
    console.log("user:", userName, "message:", message, "userId:", userId, "chatId:", chatId);

    currentChatId = localStorage.getItem('currentChatId');
    console.log("currentChatId перед непрочитанными смс:", currentChatId);

    // Проверяем, что сообщение для текущего открытого чата
    if (chatId != currentChatId || !currentChatId) {
        console.log("Сообщение для другого чата, игнорируем");

        // Помечаем чат как непрочитанный
        const chatButton = document.getElementById(chatId);
        chatButton.classList.add("unread");

        return; // Не отображаем сообщение
    }

    const msg = document.createElement("div");
    msg.textContent = `${userName}: ${message}`;
    msg.classList.add("message");

    // Определяем, кто отправил сообщение
    if (userId == currentUser) {
        msg.classList.add("from-self");
    } else {
        msg.classList.add("from-others");
    }

    document.getElementById("messages").appendChild(msg);

    //прокрутка диалога вниз
    const messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;


});

//дожидаемся загрузки страницы, только потом загружаем список чатов
document.addEventListener('DOMContentLoaded', async function ()
{
    await loadUserChats();
    await loadUserName();

    await startConnection();

   /* // После подключения — присоединяемся ко всем чатам пользователя
    const tokenUserId = localStorage.getItem('authToken');
    const response = await fetch(`/Chats/user/${tokenUserId}`);
    const chats = await response.json();

    for (const chat of chats) {
        try {
            await connection.invoke("JoinChat", { userId: Number(tokenUserId), chatId: Number(chat.id) });
            console.log("Подключен к чату:", chat.name);
        } catch (err) {
            console.error("Ошибка JoinChat при старте:", err);
        }
    }*/

    // Открываем сохраненный чат после небольшой задержки
    setTimeout(() => {
        const savedChatId = localStorage.getItem('currentChatId');
        if (savedChatId) {
            openChat(savedChatId);
        }
    }, 300);
});

async function startConnection() {
    try {
        if (connection.state !== signalR.HubConnectionState.Connected) {
            await connection.start();
            console.log("SignalR подключен");
        }
    } catch (err) {
        console.error("Ошибка подключения к SignalR:", err);
        setTimeout(startConnection, 2000); // повторная попытка
    }
}

let skip = 0;// для истории сообщений

async function loadUserName()
{
    const tokenUserId = localStorage.getItem("authToken");

    const response = await fetch(`/User/${tokenUserId}`);
    const userName = await response.text();

    console.log("currentUserName:", userName);

    const currentUserName = document.getElementById("userName");
    currentUserName.innerText = userName;
}

async function loadChatName()
{
    const chatId = localStorage.getItem("currentChatId");

    const response = await fetch(`/Chats/${chatId}`);
    const chatName = await response.json();

    console.log("currentChatName:", chatName.name);

    const currentChatName = document.getElementById("chat-title");
    currentChatName.innerText = chatName.name;
}

// Загрузка списка чатов пользователя
async function loadUserChats() {
    try {
        // Проверяем авторизацию при загрузке чатов
        const tokenUserId = localStorage.getItem('authToken');

        if (!tokenUserId) {
            window.location.href = 'login.html';
            return;
        }

        const response = await fetch(`/Chats/user/${tokenUserId}`);
        const chats = await response.json();
        
        displayChats(chats);

    } catch (error) {
        console.error('Ошибка загрузки чатов:', error);
        alert('Не удалось загрузить чаты');
    }
}

// Отображение списка чатов
function displayChats(chats) {
    const chatsList = document.getElementById('chats-list');
    chatsList.innerHTML = '';

    console.log(chats);

    if (chats.length === 0) {
        chatsList.innerHTML = '<p class="no-chats">У вас пока нет чатов</p>';
        return;
    }

    chats.forEach(chat => {
        const chatButton = document.createElement("button");
         chatButton.className = 'chat-button';
       // chatButton.classList.add('chat-button');
        chatButton.textContent = chat.name;
        chatButton.id = chat.id;
        chatButton.onclick = () => openChat(chat.id);

        chatsList.appendChild(chatButton);
    });
}

let activeButton = null; // глобальная переменная для отслеживания активной кнопки
function handleChatButtonClick(chatId)
{
    const chatButton = document.getElementById(chatId);

    // Если раньше была активная кнопка — возвращаем ей работоспособность
    if (activeButton != null && activeButton !== chatButton)
    {
        activeButton.disabled = false;
        activeButton.classList.remove("active-chatButton");
    }

    // Деактивируем новую кнопку
    chatButton.disabled = true;

    // Запоминаем её как активную
    activeButton = chatButton;
    activeButton.classList.add("active-chatButton");
}

// Открытие конкретного чата
async function openChat(chatId)
{
    localStorage.setItem('currentChatId', chatId);

    await loadChatName();
    
    const messages = document.getElementById("messages");
    messages.innerHTML = ''; // очищаем окно сообщений при открытии нового чата

    skip = 0; // при переходе в другой чат, сбрасываем счетчик пропуска сообщений для истории
    console.log("Reset skip to:", skip);

    hasMoreMessages = true; // сбрасываем флаг наличия сообщений
    isLoading = false; // сбрасываем флаг загрузки

    const chat = document.getElementById("chat");
    chat.classList.add("show");

    const chatButton = document.getElementById(chatId);
    if (chatButton)
    chatButton.classList.remove("unread"); // убираем отметку непрочитанности при открытии чата

    joinChat();

    handleChatButtonClick(chatId);
}

// Выход
function logout()
{
    localStorage.removeItem('authToken');//userId
    localStorage.removeItem('currentChatId');
    window.location.href = 'login.html';
}

//функционал определенного чата



// Проверяем авторизацию при загрузке чата
/*document.addEventListener('DOMContentLoaded', async function () {
    const token = localStorage.getItem('authToken');

    if (!token) {
        // Если токена нет, перенаправляем на страницу логина
        window.location.href = 'login.html';
        return;
    }
    await connection.start()
        .catch(err => console.error(err.toString()));//нужен await для ожидания подключения к хабу

    joinChat();
});*/

//Отображает что подключился новый участник к чату
async function joinChat()
{
    //глобальные переменные 
    currentChat = localStorage.getItem('currentChatId');
    currentUser = localStorage.getItem('authToken'); // временно, токен - это userId

    console.log("Подключаемся к чату ChatId:", currentChat);
    console.log("Пользователь id:", currentUser);

    if (!currentChat) return;

    // Ждем подключения если нужно
    if (connection.state !== signalR.HubConnectionState.Connected)
    {
        console.log("Ожидаем подключения к SignalR...");
        await startConnection();
    }

    const userId = Number(currentUser);
    const chatId = Number(currentChat);
    const payload = { userId, chatId };
    console.log("Отправляем в  JoinChat:", payload);

    try {
        await connection.invoke("JoinChat", payload);
        await loadChatHistory(currentChat);
    } catch (err) {
        console.error("Ошибка JoinChat:", err);
        // Пробуем еще раз через секунду
        setTimeout(joinChat, 1000);
    }
}

//Отправляем сообщение на сервер
function sendMessage() {
    const message = document.getElementById("messageInput").value.trim();
    console.log("Отправляем сообщение на сервер:", message);

    if (message) {
        const userId = Number(currentUser);
        const chatId = Number(currentChat);

        const payload = { userId, chatId };
        connection.invoke("SendMessage", payload, message)
            .catch(err => console.error(err.toString()));

        document.getElementById("messageInput").value = "";
    }
}




//вывод истории чата (через API)
let n = 0; //счетчик для отладки
let isLoading = false; // Флаг для отслеживания загрузки
let hasMoreMessages = true; // Флаг для отслеживания наличия сообщений

async function loadChatHistory(currentChat)
{
    // Защита от множественных одновременных вызовов
    if (isLoading || !hasMoreMessages)
        return;

    isLoading = true;

    ++n;
    console.log("loadChatHistory called", n );
    const messagesBox = document.getElementById("messages"); // контейнер для сообщений
    try {
        const res = await fetch(`/Messages/${currentChat}?skip=${skip}`);
        const messages = await res.json();

        console.log("history", messages);//проверка что приходит

        if (!Array.isArray(messages)) {
            console.warn("Received 'messages' is not an array:", messages);
            return;
        }

        if (messages.length === 0)
        {
            hasMoreMessages = false;
            console.log("Больше нет сообщений для загрузки");
            isLoading = false;
            return;
        }

        // Запоминаем высоту чата до подгрузки новых сообщений
        const previousScrollHeight = messagesBox.scrollHeight;

        messages.forEach(msg => {
            const messageDiv = document.createElement("div");
            messageDiv.textContent = `${msg.userName}: ${msg.body}`;
            messageDiv.classList.add("message");


            // стили для "своих" и "чужих" сообщений
            if (currentUser == msg.userId) {
                messageDiv.classList.add("from-self");
            }
            else {
                messageDiv.classList.add("from-others");
            }

            //вставляем новые старые сообщения в начало списка
            messagesBox.insertBefore(messageDiv, messagesBox.firstChild);
        });
        // Восстанавливаем позицию прокрутки (если были старые сообщения)
        if (messages.length > 0) {
            skip += 20; //увеличиваем skip для следующей порции сообщений при прокрутке
            console.log("Updated skip to:", skip);

            // Вычисляем разницу в высоте и корректируем прокрутку
            const newScrollHeight = messagesBox.scrollHeight;
            const heightDiff = newScrollHeight - previousScrollHeight;
            messagesBox.scrollTop += heightDiff;
        }
    }
    catch (err)
    {
        console.error("Failed to load chat history:", err);
    }
    finally
    {
        isLoading = false; // Сбрасываем флаг в любом случае
    }
}
// подписка на скролл (загрузка при прокрутке вверх)
document.getElementById("messages").addEventListener("scroll", () => {
    const messagesBox = document.getElementById("messages");
    if (messagesBox.scrollTop === 0) {
        loadChatHistory(currentChat);
    }
});

//универсальный  функционал для модальных окон
// Показываем модалку
function openModal(modal) {
    modal.style.display = "flex";
    modal.classList.add("show");
}

// Закрываем модалку
function closeModal(modal) {
    modal.classList.remove("show");
    modal.style.display = "none";
}

// Обработчик для открытия модалок
document.getElementById("createChatBtn")?.addEventListener("click", () => {
    openModal(document.getElementById("createChatModal"));
});

document.getElementById("findChatBtn")?.addEventListener("click", () => {
    openModal(document.getElementById("findChatModal"));
});

// 🔹 Один обработчик для всех крестиков
document.querySelectorAll(".close-modal").forEach(btn => {
    btn.addEventListener("click", () => {
        const modal = btn.closest(".modal");
        closeModal(modal);
    });
});

// 🔹 Закрытие по клику вне окна
window.addEventListener("click", (e) => {
    if (e.target.classList.contains("modal")) {
        closeModal(e.target);
    }
});

//---------------функционал для создания нового чата---------------------------------
// Создание нового чата
async function createNewChat()
{
    const chatName = document.getElementById("chatNameInput").value.trim();
   // const isGroup = document.getElementById("isGroupCheckbox").checked;

    if (!chatName)
    {
        alert("Введите название чата");
        return;
    }

    const tokenUserId = localStorage.getItem('authToken');
    try
    {
        const response = await fetch('/Chats',
            {
                method: 'POST',
                headers:
                {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(
                    {
                        name: chatName,
                        userId: tokenUserId
                    })
            });

        if (!response.ok)
        {
            throw new Error(`Ошибка: ${response.status}`);
        }

        const data = await response.text();
        console.log("Чат создан:", data);

        // Закрываем окно после создания
        createChatModal.style.display = "none";
        // Обновляем список чатов
        location.reload();
    }
    catch (err)
    {
        console.error("Не удалось создать чат:", err);
        alert("Ошибка при создании чата, попробуйте снова.");
    }
}

const submitChatBtn = document.getElementById("submitChatBtn");
//навешиваем обработчик на кнопку создания чата
submitChatBtn.addEventListener("click", createNewChat);


//---------------функционал для поиска чата--------------------------------
async function findChat()
{
    const chatName = document.getElementById("findChatNameInput").value.trim();
    console.log("Ищем чат по имени:", chatName);

    if (!chatName)
    {
        alert("Введите название чата");
        return;
    }
    try
    {
        const response = await fetch(`/Chats/by-name/${encodeURIComponent(chatName)}`);

        if (!response.ok)
        {
            if (response.status === 404)
            {
                alert("Чат не найден");
                return;
            }
            throw new Error(`Ошибка: ${response.status}`);
        }

        const chat = await response.json();
        console.log("Найден чат:", chat.name);

        // Открываем найденный чат
        openChat(chat.id);
        // Закрываем окно после поиска
        findChatModal.style.display = "none";
        findChatModal.classList.remove("show");

        // Обновляем список чатов
        const tokenUserId = localStorage.getItem('authToken');
        const userId = Number(tokenUserId);
        const chatId = Number(chat.id);
        console.log("Добавляем пользователя в чат: userId:", userId, "chatId:", chatId);
        const responseListChats = await fetch(`/Chats/${chatId}/add-user/${userId}`,
            {
                method: 'POST',
                headers:
                {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({}) 
            });

    }
    catch (err)
    {
        console.error("Не удалось найти чат:", err);
        alert("Ошибка при поиске чата, попробуйте снова.");
    }

    window.location.reload();
}

const searchChatBtn = document.getElementById("searchChatBtn");
//навешиваем обработчик на кнопку поиска чата
searchChatBtn.addEventListener("click", findChat);


