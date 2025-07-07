import store from "@/store";
import moment from "moment/moment";

export default {
    formatDate(date) {
        if (store.state.userProfile.language === "en") {
            return moment(date).format("YYYY/MM/DD");
        } else {
            return moment(date).format("DD/MM/YYYY");
        }
    },
}