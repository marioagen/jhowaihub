
class LogRService {

     showMessage(msg) {

         let envType = ENV_CONFIG.VUE_APP_ENV_TYPE;

         if (envType === "Development") {

             console.log(msg);
         }
    }
}
const logService = new LogRService();
export default logService;