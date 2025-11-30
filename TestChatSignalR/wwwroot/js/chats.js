const userIdcon = localStorage.getItem('authToken');

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`/chat?userId=${userIdcon}`)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();



//получаем сообщение на клиенте что кто-то подключился к чату, либо вышел из чата
connection.on("ReceiveSystemMessage", (message, chatId) =>
{
    const currentChatId = localStorage.getItem('currentChatId');

    // Проверяем, что сообщение для текущего открытого чата
    if (chatId != currentChatId) {
        console.log("Сообщение system для другого чата, игнорируем");
        return; // Не отображаем сообщение
    }

    const msg = document.createElement("div");
    msg.textContent =message;
    msg.classList.add("message");
    msg.classList.add("system-message");
    document.getElementById("messages").appendChild(msg);
    
    setTimeout(() => { msg.remove(); }, 3000);

    //прокрутка диалога вниз
    const messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});

let isChatCreating = true;
let pendingMessages = []; // буфер для сообщений, пришедших раньше чата
//Получаем сообщение от usera на клиенте
connection.on("ReceiveMessage", (message, userId, chatId) =>
{
    console.log("Получаем сообщение на клиенте");
    console.log("message:", message, "userId:", userId, "chatId:", chatId);

    const currentChatId = localStorage.getItem('currentChatId');
    console.log("ReceiveMessage ChatId перед непрочитанными смс:", currentChatId);

    // Если чат в процессе создания, сохраняем сообщение в буфер
    if (isChatCreating) {
        console.log("Чат создается, откладываем сообщение");
        pendingMessages.push({ message, userId, chatId });
        return;
    }

    renderMessage(message, userId, chatId);
});

connection.on("CreatedChat", async (newChatId) =>
{
    console.log("Сервер создал чат  с id:", newChatId);
    // Обновляем список чатов
    await renderChatList();

    if (localStorage.getItem('receiverId'))//условие для sender
    {
        handleChatButtonClick(newChatId);//делаем кнопку нового чата активной
        localStorage.removeItem('receiverId');
        localStorage.setItem('currentChatId', newChatId);//сохраняем id нового созданного чата, чтобы он сразу открылся
    }

    // Снимаем блокировку и обрабатываем отложенные сообщения
    isChatCreating = false;
    processPendingMessages();
});

function processPendingMessages() {
    while (pendingMessages.length > 0) {
        const { message, userId, chatId } = pendingMessages.shift();
        renderMessage(message, userId, chatId);
    }
}

//дожидаемся загрузки страницы, только потом загружаем список чатов
document.addEventListener('DOMContentLoaded', async function ()
{
    // Проверяем наличие токена авторизации
    const token = localStorage.getItem('authToken');

    if (!token) {
        // Если токена нет, перенаправляем на страницу логина
        window.location.href = 'login.html';
        return;
    }

    await renderChatList();
    await loadUserName();

    await startConnection();

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
    const currentUserId = localStorage.getItem("authToken");

    const response = await fetch(`/User/by-id/${currentUserId}`);
    const userName = await response.text();

    console.log("currentUserName:", userName);

    const userNameEl = document.getElementById("userName");
    userNameEl.innerText = userName;
}

let chatsCashe = null;
// Загрузка списка чатов пользователя
async function renderChatList() {
    try {
        // Проверяем авторизацию при загрузке чатов
        const tokenUserId = localStorage.getItem('authToken');

        const response = await fetch(`/Chats/user/${tokenUserId}`);
        const chats = await response.json();

        chatsCashe = chats;

        displayChats(chats);
    } catch (error) {
        console.error('Ошибка загрузки чатов:', error);
        alert('Не удалось загрузить чаты');
    }
    
    //триггерим событие после полной отрисовки для навешивания отметки непрочитанности
   // document.dispatchEvent(new Event("chatListRendered"));
}

function renderMessage(message, userId, chatId)
{
    const currentChatId = localStorage.getItem('currentChatId');
    console.log("renderMessage on ChatId перед непрочитанными смс:", currentChatId);

    if (chatId != currentChatId || !currentChatId) {
        console.log("Сообщение для другого чата, игнорируем");
        const chatButton = document.getElementById(chatId);
        if (chatButton) {
            chatButton.classList.add("unread");
        }
        return;
    }

    const msg = document.createElement("div");
    msg.textContent = message;
    msg.classList.add("message");

    const currentUserId = localStorage.getItem('authToken');

    // Определяем, кто отправил сообщение
    if (userId == currentUserId) {
        msg.classList.add("from-self");
    } else {
        msg.classList.add("from-others");
    }

    document.getElementById("messages").appendChild(msg);

    //прокрутка диалога вниз
    const messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
}

async function loadChatName()
{
    const currentChatId = localStorage.getItem("currentChatId");

    const chat = chatsCashe.find(c => c.id == currentChatId);

    console.log("currentChatName:", chat.displayName);

    const chatNameEl = document.getElementById("chat-title");
    chatNameEl.innerText = chat.displayName;
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
        chatButton.textContent = chat.displayName;
        chatButton.id = chat.id;
        chatButton.onclick = () => openChat(chat.id);

        chatsList.appendChild(chatButton);
    });
}

let activeButton = null; // глобальная переменная для отслеживания активной кнопки
async function handleChatButtonClick(chatId)
{
    console.log("handleChatButtonClick for chatId:", chatId);
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
    console.log("openChat with id:", chatId);
    localStorage.setItem('currentChatId', chatId);

    localStorage.removeItem('receiverId');//удаляем receiverId при открытии существующего чата, чтобы не ломал поведение программы

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

// открытие чата с новым пользователем
async function openNewChat(userName)
{
    console.log("openChat without id:");

    const chatNameEl = document.getElementById("chat-title");
    chatNameEl.innerText = userName;
   // localStorage.setItem('currentChatId', chatId);

    //await loadChatName();

    const messages = document.getElementById("messages");
    messages.innerHTML = ''; // очищаем окно сообщений при открытии нового чата

    skip = 0; // при переходе в другой чат, сбрасываем счетчик пропуска сообщений для истории
    console.log("Reset skip to:", skip);

   // hasMoreMessages = true; // сбрасываем флаг наличия сообщений
    //isLoading = false; // сбрасываем флаг загрузки

    const chat = document.getElementById("chat");
    chat.classList.add("show");

   // const chatButton = document.getElementById(chatId);
    //if (chatButton) chatButton.classList.remove("unread"); // убираем отметку непрочитанности при открытии чата

    //joinChat();

   // handleChatButtonClick(chatId);
}

// Выход
function logout()
{
    localStorage.removeItem('authToken');//userId
    localStorage.removeItem('currentChatId');
    localStorage.removeItem('receiverId');
    localStorage.removeItem('theme');

    window.location.href = 'login.html';
}


//Отображает что подключился новый участник к чату
async function joinChat()
{
    //глобальные переменные 
   const currentChatId = localStorage.getItem('currentChatId');
   const currentUserId = localStorage.getItem('authToken'); // временно, токен - это userId

    console.log("Подключаемся к чату ChatId:", currentChatId);
    console.log("Пользователь id:", currentUserId);

    if (!currentChatId) return;

    // Ждем подключения если нужно
    if (connection.state !== signalR.HubConnectionState.Connected)
    {
        console.log("Ожидаем подключения к SignalR...");
        await startConnection();
    }

    const userId = Number(currentUserId);
    const chatId = Number(currentChatId);
    const payload = { userId, chatId };
    console.log("Отправляем в  JoinChat:", payload);

    try {
        await connection.invoke("JoinChat", payload);
        await loadChatHistory(currentChatId);
    } catch (err) {
        console.error("Ошибка JoinChat:", err);
        // Пробуем еще раз через секунду
        setTimeout(joinChat, 1000);
    }
}

//Отправляем сообщение на сервер
 async function sendMessage() {
    const message = document.getElementById("messageInput").value.trim();
    console.log("Отправляем сообщение на сервер:", message);

    if (message)
    {
        let chatId = Number(localStorage.getItem('currentChatId'));
        const senderId = Number(localStorage.getItem('authToken'));
        const receiverId = Number(localStorage.getItem('receiverId'));

        if (receiverId) {
            chatId = 0; //если есть receiverId, значит чат новый, и chatId не нужен
            console.log("Новый чат с пользователем receiverId:", receiverId);

            // Устанавливаем флаг создания чата
            isChatCreating = true;
        }

        const payload = { userId: senderId, chatId: chatId };

        //получаем chatId новый или существующий
        const returnedChatId = await connection.invoke("SendMessage", payload, receiverId, message);
        console.log(" returnedChatId:", returnedChatId);

        //сохраняем сразу chatId чтобы потом методы/events точно получили актуальный id
        if (returnedChatId) {
            localStorage.setItem('currentChatId', String(returnedChatId));
            console.log("sendMessage new chatId:", returnedChatId);
            localStorage.removeItem('receiverId');
        }

        document.getElementById("messageInput").value = "";
    }
}




//вывод истории чата (через API)
let n = 0; //счетчик для отладки
let isLoading = false; // Флаг для отслеживания загрузки
let hasMoreMessages = true; // Флаг для отслеживания наличия сообщений

async function loadChatHistory(currentChatId)
{
    // Защита от множественных одновременных вызовов
    if (isLoading || !hasMoreMessages)
        return;

    isLoading = true;

    ++n;
    console.log("loadChatHistory called", n );
    const messagesBox = document.getElementById("messages"); // контейнер для сообщений
    try {
        const response = await fetch(`/Messages/${currentChatId}?skip=${skip}`);
        const messages = await response.json();

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
            messageDiv.textContent = msg.body;
            messageDiv.classList.add("message");

            const currentUserId = localStorage.getItem('authToken'); 

            // стили для "своих" и "чужих" сообщений
            if (currentUserId == msg.userId) {
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
    if (messagesBox.scrollTop === 0)
    {
        const currentChatId = localStorage.getItem('currentChatId');
        loadChatHistory(currentChatId);
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

    modal.querySelectorAll("input").forEach(input => input.value = "");
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
/*async function createNewChat()
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
submitChatBtn.addEventListener("click", createNewChat);*/


//---------------функционал для поиска другого пользователя--------------------------------
async function findUser()
{
    const userNameInput = document.getElementById("findUserNameInput");
    userName = userNameInput.value.trim();
    console.log("Ищем user по имени:", userName);

    if (!userName)
    {
        alert("Введите имя пользователя");
        return;
    }

    try
    {
        const response = await fetch(`/User/by-name/${encodeURIComponent(userName)}`);

        if (!response.ok)
        {
            if (response.status === 404)
            {
                alert("Пользователь не найден");
                return;
            }
            throw new Error(`Ошибка: ${response.status}`);
        }

        const foundUser = await response.json();
        localStorage.setItem('receiverId', foundUser.receiverId);
        console.log("Найден user:", foundUser);

        // Открываем чат c найденным пользователем
        openNewChat(foundUser.userName);
        // Закрываем окно после поиска
        findChatModal.style.display = "none";
        findChatModal.classList.remove("show");

        userNameInput.value = "";

        // Обновляем список чатов
      //  const tokenUserId = localStorage.getItem('authToken');
        //const userId = Number(tokenUserId);
        //const chatId = Number(chat.id);

              //перепроверить надо
      /*  const responseListChats = await fetch(`/Chats/${chatId}/add-user/${userId}`,
            {
                method: 'POST',
                headers:
                {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({}) 
            });*/

    }
    catch (err)
    {
        console.error("Не удалось найти чат:", err);
        alert("Чат не найден");
    }

   // window.location.reload();
}

const searchChatBtn = document.getElementById("searchChatBtn");
//навешиваем обработчик на кнопку поиска чата
searchChatBtn.addEventListener("click", findUser);


