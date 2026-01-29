import * as signalR from "@microsoft/signalr";
import logService from "@/services/log/logService.js";
import store from "@/store";

class SignalRService {
    constructor() {
        this.connection = null;
        this.hubUrl = `${ENV_CONFIG.VUE_APP_BASE_URL_API}/hubs/notifications`;
    }

    async startConnection() {
        if (this.connection) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                withCredentials: false,
                accessTokenFactory: () =>
                    store.state.userProfile.tokenApi,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.None)
            .build();

        try {
            await this.connection.start();
        } catch (error) {
            logService.showMessage(
                "Error starting SignalR connection:" + error
            );
        }
    }

    stopConnection() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }

    on(eventName, callback) {
        if (this.connection) {
            this.connection.on(eventName, callback);
        }
    }

    off(eventName) {
        if (this.connection) {
            this.connection.off(eventName);
        }
    }

    async send(eventName, ...args) {
        if (this.connection) {
            try {
                await this.connection.invoke(
                    eventName,
                    ...args
                );
            } catch (error) {
                logService.showMessage(
                    `Error sending SignalR event '${eventName}':` +
                        error
                );
            }
        }
    }
}

const signalRService = new SignalRService();
export default signalRService;
