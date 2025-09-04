import * as signalR from "@microsoft/signalr";
import logService from '@/services/log/logService.js';
import store from '@/store';

class SignalRService {
    constructor() {
        this.connection = null;
        this.hubUrl = `${ENV_CONFIG.VUE_APP_BASE_URL_API}/hubs/notifications`;
    }

    async startConnection(options = {}) {
        if (this.connection) {
            logService.showMessage("SignalR connection already exists.");
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                withCredentials: false, 
                accessTokenFactory: () => store.state.userProfile.tokenApi,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Debug)
            .build();

        this.connection.onreconnecting(() => {
            logService.showMessage("SignalR reconnecting...");
        });

        this.connection.onreconnected(() => {
            logService.showMessage("SignalR reconnected");
        });

        this.connection.onclose(() => {
            logService.showMessage("SignalR connection closed");
        });

        try {
            await this.connection.start();
            logService.showMessage("SignalR connection started");
        } catch (error) {
            logService.showMessage("Error starting SignalR connection:" + error);
        }
    }

    stopConnection() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
            logService.showMessage("SignalR connection stopped.");
        } else {
            logService.showMessage("No SignalR connection to stop.");
        }
    }

    on(eventName, callback) {
        if (this.connection) {
            this.connection.on(eventName, callback);
        } else {
            logService.showMessage("SignalR connection is not established.");
        }
    }

    off(eventName) {
        if (this.connection) {
            this.connection.off(eventName);
        } else {
            logService.showMessage("SignalR connection is not established.");
        }
    }

    async send(eventName, ...args) {
        if (this.connection) {
            try {
                await this.connection.invoke(eventName, ...args);
            } catch (error) {
                logService.showMessage(`Error sending SignalR event '${eventName}':` +error);
            }
        } else {
            logService.showMessage("SignalR connection is not established.");
        }
    }
}

const signalRService = new SignalRService();
export default signalRService;