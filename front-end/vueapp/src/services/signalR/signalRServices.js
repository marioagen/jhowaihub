import * as signalR from "@microsoft/signalr";
import logService from "@/services/log/logService.js";
import store from "@/store";

const RECONNECT_INTERVALS_MS = [0, 2000, 5000, 10000, 20000, 30000, 60000];

class InfiniteRetryPolicy {
    nextRetryDelayInMilliseconds(retryContext) {
        const index = Math.min(retryContext.previousRetryCount, RECONNECT_INTERVALS_MS.length - 1);
        return RECONNECT_INTERVALS_MS[index];
    }
}

class SignalRService {
    constructor() {
        this.connection = null;
        this.hubUrl = `${ENV_CONFIG.VUE_APP_BASE_URL_API}/hubs/notifications`;
        this._intentionalStop = false;
        this._registeredHandlers = new Map();
    }

    async startConnection() {
        if (this._isConnectedOrConnecting()) return;

        this._intentionalStop = false;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                withCredentials: false,
                accessTokenFactory: () => store.state.userProfile.tokenApi,
            })
            .withAutomaticReconnect(new InfiniteRetryPolicy())
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        this._registerLifecycleHandlers();
        this._reattachRegisteredHandlers();

        try {
            await this.connection.start();
        } catch (error) {
            logService.showMessage("Error starting SignalR connection: " + error);
        }
    }

    stopConnection() {
        this._intentionalStop = true;
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }

    on(eventName, callback) {
        this._registeredHandlers.set(eventName, callback);
        if (this.connection) {
            this.connection.on(eventName, callback);
        }
    }

    off(eventName) {
        this._registeredHandlers.delete(eventName);
        if (this.connection) {
            this.connection.off(eventName);
        }
    }

    async send(eventName, ...args) {
        if (!this.connection) return;
        try {
            await this.connection.invoke(eventName, ...args);
        } catch (error) {
            logService.showMessage(`Error sending SignalR event '${eventName}': ` + error);
        }
    }

    _isConnectedOrConnecting() {
        if (!this.connection) return false;
        const state = this.connection.state;
        return (
            state === signalR.HubConnectionState.Connected ||
            state === signalR.HubConnectionState.Connecting ||
            state === signalR.HubConnectionState.Reconnecting
        );
    }

    _registerLifecycleHandlers() {
        this.connection.onreconnecting(() => {
            logService.showMessage("SignalR reconnecting...");
        });

        this.connection.onreconnected(() => {
            logService.showMessage("SignalR reconnected.");
        });

        this.connection.onclose((error) => {
            if (this._intentionalStop) return;

            logService.showMessage("SignalR connection closed. Restarting in 5s...");
            setTimeout(() => {
                this.connection = null;
                this.startConnection();
            }, 5000);
        });
    }

    _reattachRegisteredHandlers() {
        for (const [eventName, callback] of this._registeredHandlers) {
            this.connection.on(eventName, callback);
        }
    }
}

const signalRService = new SignalRService();
export default signalRService;
