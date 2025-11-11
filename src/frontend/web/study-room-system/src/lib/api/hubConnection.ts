import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { baseUrl } from "../utils";

let connection: HubConnection | null = null;

function getHubConnection() {
    if (!connection) {
        connection = new HubConnectionBuilder()
            .withUrl(`${baseUrl}/hub/data`, { accessTokenFactory: () => `${localStorage.getItem("token")}` })
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