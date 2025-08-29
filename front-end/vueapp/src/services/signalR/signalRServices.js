import * as signalR from "@microsoft/signalr";
import store from '@/store';

class SignalRService {
    constructor() {
        this.connection = null;
        this.hubUrl = `${ENV_CONFIG.VUE_APP_BASE_URL_API}/hubs/notifications`;
    }

    async startConnection(options = {}) {
        if (this.connection) {
            console.warn("SignalR connection already exists.");
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
            console.log("SignalR reconnecting...");
        });

        this.connection.onreconnected(() => {
            console.log("SignalR reconnected.");
        });

        this.connection.onclose(() => {
            console.log("SignalR connection closed.");
        });

        try {
            await this.connection.start();
            console.log("SignalR connection started.");
        } catch (error) {
            console.error("Error starting SignalR connection:", error);
        }
    }

    stopConnection() {
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
            console.log("SignalR connection stopped.");
        } else {
            console.warn("No SignalR connection to stop.");
        }
    }

    on(eventName, callback) {
        if (this.connection) {
            this.connection.on(eventName, callback);
        } else {
            console.warn("SignalR connection is not established.");
        }
    }

    off(eventName) {
        if (this.connection) {
            this.connection.off(eventName);
        } else {
            console.warn("SignalR connection is not established.");
        }
    }

    async send(eventName, ...args) {
        if (this.connection) {
            try {
                await this.connection.invoke(eventName, ...args);
            } catch (error) {
                console.error(`Error sending SignalR event '${eventName}':`, error);
            }
        } else {
            console.warn("SignalR connection is not established.");
        }
    }
}

const signalRService = new SignalRService();
export default signalRService;