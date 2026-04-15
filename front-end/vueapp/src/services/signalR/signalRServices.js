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
        this._startRetryTimer = null;
        this._visibilityHandler = this._onVisibilityChange.bind(this);
        document.addEventListener("visibilitychange", this._visibilityHandler);
    }

    async startConnection() {
        if (this._intentionalStop) return;
        if (this._isConnectedOrConnecting()) return;

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
            logService.showMessage("SignalR start failed. Retrying in 5s: " + error);
            this._scheduleRestart();
        }
    }

    stopConnection() {
        this._intentionalStop = true;
        this._clearRestartTimer();
        document.removeEventListener("visibilitychange", this._visibilityHandler);
        if (this.connection) {
            this.connection.stop();
            this.connection = null;
        }
    }

    on(eventName, callback) {
        if (!this._registeredHandlers.has(eventName)) {
            this._registeredHandlers.set(eventName, new Set());
        }
        this._registeredHandlers.get(eventName).add(callback);

        if (this.connection) {
            this.connection.on(eventName, callback);
        }
    }

    off(eventName, callback) {
        const callbacks = this._registeredHandlers.get(eventName);
        if (callbacks) {
            if (callback) {
                callbacks.delete(callback);
                if (callbacks.size === 0) {
                    this._registeredHandlers.delete(eventName);
                }
            } else {
                this._registeredHandlers.delete(eventName);
            }
        }

        if (this.connection) {
            callback
                ? this.connection.off(eventName, callback)
                : this.connection.off(eventName);
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
            this._scheduleRestart();
        });
    }

    _scheduleRestart(delayMs = 5000) {
        this._clearRestartTimer();
        this._startRetryTimer = setTimeout(() => {
            this._startRetryTimer = null;
            this.connection = null;
            this.startConnection();
        }, delayMs);
    }

    _clearRestartTimer() {
        if (this._startRetryTimer) {
            clearTimeout(this._startRetryTimer);
            this._startRetryTimer = null;
        }
    }

    _reattachRegisteredHandlers() {
        for (const [eventName, callbacks] of this._registeredHandlers) {
            for (const callback of callbacks) {
                this.connection.on(eventName, callback);
            }
        }
    }

    _onVisibilityChange() {
        if (document.visibilityState !== "visible") return;
        if (this._intentionalStop) return;
        if (!this._isConnectedOrConnecting()) {
            logService.showMessage("SignalR: tab visible, reconnecting...");
            this.connection = null;
            this.startConnection();
        }
    }
}

const signalRService = new SignalRService();
export default signalRService;
