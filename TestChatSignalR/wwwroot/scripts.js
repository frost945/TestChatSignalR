
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(err => console.error(err.toString()));

//Отображает что подключился новый участник к чату
function joinChat()
{
    currentUser = document.getElementById("username").value.trim();
    currentChat = document.getElementById("chatname").value.trim();

    console.log("Подключаемся к чату", currentUser, currentChat);

    if (currentUser && currentChat)
    {
        document.getElementById("login").style.display = "none";
        document.getElementById("chat").style.display = "block";
        document.getElementById("chat-title").innerText = `Чат: ${currentChat}`;

        // Отправляем данные пользователя на сервер
        connection.invoke("joinChat", { userName: currentUser, chatName: currentChat })
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
    console.log("Отправляем сообщение на сервер");

    const message = document.getElementById("messageInput").value.trim();
    if (message)
    {
        connection.invoke("sendMessage", { userName: currentUser, chatName: currentChat }, message)
            .catch(err => console.error(err.toString()));

        document.getElementById("messageInput").value = "";
    }
}


//Получаем сообщение на клиенте
connection.on("receiveMessage", (userName, chatName, message) =>
{
    console.log("Получаем сообщение на клиенте");
    console.log("chat:", currentChat);
    console.log("user:", currentUser);

    if (chatName === currentChat)
    {
        const msg = document.createElement("div");
        msg.textContent = `${userName}: ${message}`;
        msg.classList.add("message");

        // Определяем, кто отправил сообщение
        if (userName === currentUser) {
            msg.classList.add("from-self");
        } else {
            msg.classList.add("from-others");
        }

        document.getElementById("messages").appendChild(msg);

        //прокрутка диалога вниз
        const messagesDiv = document.getElementById("messages");
        messagesDiv.scrollTop = messagesDiv.scrollHeight;

    }
});

//вывод истории чата
let skip = 0;
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
});

//Загружаем историю чата при прокрутке вверх
historyChatBox.addEventListener('scroll', ()=>
{
    if (historyChatBox.scrollTop === 0)
    {
        skip += 20;
        console.log("skip:", skip);
        connection.invoke("loadChatHistory", currentChat, skip)
        .catch (err => console.error("Failed to load history:", err));
    }
});
