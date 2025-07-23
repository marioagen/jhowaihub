import api from "@/services/api";

export default {
    AuthenticateUser(form, userAzure) {
        return api.post("/Account/Authenticate", form, {
            headers: { Authorization: `Bearer ${userAzure}` },
        })
            .then(({ data }) => {
                return {
                    tokenApi: data.token,
                    tenant: data.tenant,
                };
            })
            .catch(() => {
                return {
                    error: "Error"
                }
            });
    },
    GetClientId() {
        return api.get("/Account/clientId")
            .then(({ data }) => {
                return data;
            })
            .catch(() => {
                return {
                    error: "Error"
                }
            });
    },
    Login(credentials) {
        return credentials;
        // return api.post("", credentials)
        //     .then(({ data }) => {
        //         console.log(data)
        //     })
        //     .catch(() => {
        //         return {
        //             erro: "Error",
        //         }
        //     });
    },
};