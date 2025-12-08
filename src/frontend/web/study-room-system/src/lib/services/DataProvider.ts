import { ref, type Ref } from "vue";
import { getHubConnection, restartHubConnection } from "../api/hubConnection";
import type { Room } from "../types/Room";
import type { User } from "../types/User";
import type { Booking } from "../types/Booking";

class DataProvider {

    private _isStart = false;
    private messageHandler : { message:string, handler : (...args : any[]) => void }[] = [
        {message : 'data-change',handler: (payload: any,data : any) =>{
            console.log(payload,data);
        }}
    ];

    public readonly user = ref<User | null>(null);
    public readonly rooms = ref<Room[]>([]);
    public readonly myBookings = ref<Booking[]>([]);



    /**
     * 启动
     */
    public async start() {
        if (this._isStart) return;
        this._isStart = true;
        await restartHubConnection();
        this.listenMessage();
    }





    public async close() {
        if(!this._isStart) return;
        this._isStart = false;

        this.unlistenMessage();
        getHubConnection().stop();
    }

    private listenMessage(){
        const hubConnection = getHubConnection();
        this.messageHandler.forEach(item => {
            hubConnection.on(item.message, item.handler);
        });
    }

    private unlistenMessage(){
        const hubConnection = getHubConnection();
        this.messageHandler.forEach(item => {
            hubConnection.off(item.message, item.handler);
        });
    }
}

const dataProvider = new DataProvider();
export { dataProvider };