import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const baseUrl = import.meta.env.DEV ? 'http://localhost:5106' : '';
let connection: HubConnection | null = null;

function getHubConnection() {
    if (!connection) {
        connection = new HubConnectionBuilder()
            .withUrl(`${baseUrl}/hub/data`, { })
            // .withUrl(`/hub/data`, { accessTokenFactory: () => `${localStorage.getItem("token")}` })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();
    }
    return connection;
}

async function restartHubConnection() {
    if (connection) {
        await connection.stop();
    }
    await getHubConnection().start().catch(console.error);
}


export { getHubConnection, restartHubConnection };