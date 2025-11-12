import { restartHubConnection } from "../api/hubConnection";

class DataProvider {

    /**
     * 启动
     */
    public async start(){
        await restartHubConnection();
    }



    public async close(){

    }
}

const dataProvider = new DataProvider();
export { dataProvider };