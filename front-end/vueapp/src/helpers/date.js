import store from "@/store";
import { format, parseISO, isValid } from "date-fns";

export default {
    formatDate(date) {
       if (!date) return "";
        
        let dateObj;

        if (typeof date === "string") {
            dateObj = parseISO(date);
            
            if (!isValid(dateObj)) {
                dateObj = new Date(date);
            }
        } else if (date instanceof Date) {
            dateObj = date;
        } else {
            dateObj = new Date(date);
        }
        
        if (!isValid(dateObj)) {
            console.warn("Invalid date provided to formatDate:", date);
            return "";
        }
        
        if (store.state.userProfile.language === "en") {
            return format(dateObj, "yyyy/MM/dd");
        } else {
            return format(dateObj, "dd/MM/yyyy");
        }
    },
};
