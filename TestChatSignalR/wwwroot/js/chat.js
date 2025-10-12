
/*const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Проверяем авторизацию при загрузке чата
document.addEventListener('DOMContentLoaded', async function ()
{
    const token = localStorage.getItem('authToken');

    if (!token) {
        // Если токена нет, перенаправляем на страницу логина
        window.location.href = 'login.html';
        return;
    }
    await connection.start()
        .catch(err => console.error(err.toString()));//нужен await для ожидания подключения к хабу

    joinChat();
});

//Отображает что подключился новый участник к чату
async function joinChat()
{
    //глобальные переменные 
     currentChat = localStorage.getItem('currentChatId');
     currentUser = localStorage.getItem('authToken'); // временно, токен - это userId

    console.log("Подключаемся к чату ChatId:", currentChat);
    console.log("Пользователь id:", currentUser);

    if (currentChat)
    {
        const userId = Number(currentUser);
        const chatId = Number(currentChat);

        const payload = { userId, chatId };
        console.log("Отправляем в joinChat:", payload);

        // Отправляем данные пользователя на сервер
        connection.invoke("JoinChat", payload) 
            .then(() => loadChatHistory(currentChat)) // Загружаем историю чата после подключения
            .catch(err => console.error(err.toString()));
    }
    else
    {
        alert("Введите имя и название чата");
    }
}

//Отправляем сообщение на сервер
function sendMessage()
{
    const message = document.getElementById("messageInput").value.trim();
    console.log("Отправляем сообщение на сервер:", message);

    if (message)
    {
        const userId = Number(currentUser);
        const chatId = Number(currentChat);

        const payload = { userId, chatId };
        connection.invoke("SendMessage", payload, message)
            .catch(err => console.error(err.toString()));

         document.getElementById("messageInput").value = "";
    }
}

//получаем сообщение на клиенте что кто-то подключился к чату
connection.on("ReceiveSystemMessage", (chatName, message) =>
{
    //обновляем название чата
    const currentChatName = document.getElementById("chat-title");
    currentChatName.innerText = chatName;
    console.log("currentChatName:", chatName);

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
connection.on("ReceiveMessage", ( userName, message, userId) =>
{
    console.log("Получаем сообщение на клиенте");
    console.log("user:", userName, "message:", message, "userId:", userId);
    
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


//вывод истории чата (через API)
let skip = 0;
async function loadChatHistory(currentChat)
{
    const historyChatBox = document.getElementById("messages"); // контейнер для сообщений
    try
    {
        const res = await fetch(`/Messages/${currentChat}?skip=${skip}`);
        const messages = await res.json();

        console.log("history", messages);//проверка что приходит

        if (!Array.isArray(messages))
        {
            console.warn("Received 'messages' is not an array:", messages);
            return;
        }

        // Запоминаем высоту чата до подгрузки новых сообщений
        const previousScrollHeight = historyChatBox.scrollHeight;

        messages.forEach(msg =>
        {
            const messageDiv = document.createElement("div");
            messageDiv.textContent = `${msg.userName}: ${msg.body}`;
            messageDiv.classList.add("message");


            // стили для "своих" и "чужих" сообщений
            if (currentUser == msg.userId)
            {
                messageDiv.classList.add("from-self");
            }
            else
            {
                messageDiv.classList.add("from-others");
            }

            //вставляем новые старые сообщения в начало списка
            historyChatBox.insertBefore(messageDiv, historyChatBox.firstChild);
        });
        // Восстанавливаем позицию прокрутки (если были старые сообщения)
        if (messages.length > 0)
        {
            skip += 20; //увеличиваем skip для следующей порции сообщений при прокрутке

            const newScrollHeight = historyChatBox.scrollHeight;
            const heightDiff = newScrollHeight - previousScrollHeight;
            historyChatBox.scrollTop += heightDiff;
        }
    }

    catch (err)
    {
        console.error("Failed to load chat history:", err);
    }
}
// подписка на скролл (загрузка при прокрутке вверх)
document.getElementById("messages").addEventListener("scroll", () =>
{
    const historyChatBox = document.getElementById("messages");
    if (historyChatBox.scrollTop === 0)
    {
        loadChatHistory(currentChat);
    }
});*/

//--------------------------------------------------------
//вывод истории чата (вариант через SignalR в хабе)
/*let skip = 0;
const historyChatBox = document.getElementById("messages");

connection.on("receiveHistory", (messages) =>
{
    console.log("receiveHistory triggered!");
    console.log("Chat ID:", currentChat);
    console.log("Messages received:", messages.length);
    console.log("user:", currentUser);

    // Запоминаем высоту чата до подгрузки новых сообщений
    const previousScrollHeight = historyChatBox.scrollHeight;

    if (Array.isArray(messages))
    {
        messages.forEach(msg =>
        {
            const messageDiv = document.createElement("div");
            messageDiv.textContent = `${msg.userName}: ${msg.message}`;
            messageDiv.classList.add("message");

            // Определяем, кто отправил сообщение
            currentUser === msg.userName ? messageDiv.classList.add("from-self") : messageDiv.classList.add("from-others");
            //вставляем новые старые сообщения в начало списка
            historyChatBox.insertBefore(messageDiv, historyChatBox.firstChild);
        });
    }
    else
    {
        console.warn("Received 'messages' is not an array:", messages);
    }

    // Восстанавливаем позицию прокрутки (если были старые сообщения)
    if (messages.length > 0)
    {
        const newScrollHeight = historyChatBox.scrollHeight;
        const heightDiff = newScrollHeight - previousScrollHeight;
        historyChatBox.scrollTop += heightDiff;
    }  
});*/



//Загружаем историю чата при прокрутке вверх (вариант через SignalR в хабе)
/*historyChatBox.addEventListener('scroll', ()=>
{
    if (historyChatBox.scrollTop === 0)
    {
        skip += 20;
        console.log("skip:", skip);
        connection.invoke("loadChatHistory", currentChat, skip)
        .catch (err => console.error("Failed to load history:", err));
    }
});*/
