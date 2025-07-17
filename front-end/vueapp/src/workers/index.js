import GlobalEventService from "../services/globalEventService";
const worker = new Worker(new URL("./uploadFileWorker.js", import.meta.url));

//Receives the post events from the uploadFileWorker.js file
//to send to the notification component in the main thread
worker.onmessage = (event) => {
    const { type, success, nameFile, chunks, namesFiles } = event.data;
    if (type === "uploadComplete") {
        GlobalEventService.emit("uploadComplete", { success, nameFile, chunks });
    } else if (type === "uploadInProgress" && success) {
        GlobalEventService.emit("uploadInProgress", { success, nameFile, chunks });
    } else if (type === "uploadStarted" && success) {
        GlobalEventService.emit("uploadStarted", { success, namesFiles });
    }
};

const send = (message) =>
    worker.postMessage({
        message,
    });

export default {
    worker,
    send,
};
