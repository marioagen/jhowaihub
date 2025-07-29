<template>
    <div class="modal fade show" id="novoUsuarioModal" tabindex="-1" ref="modal">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ $t("labelNewUser") }}
                        <small class="text-muted d-block text-sm">{{ $t("labelNewTeamUserMessage") }}</small>
                    </h6>
                    <button type="button" class="btn-close" @click="close"></button>
                </div>
                <Form @submit="handleSubmit" ref="form">
                    <div class="modal-body">                    
                         <div class="row">
                            <div class="col-6">
                        <div class="mb-3">
                            <label for="name" class="form-label fw-semibold mb-0">{{ $t("labelName") }}</label>
                            <Field
                                name="name"
                                type="text"
                                class="form-control form-control-sm"
                                :placeholder="$t('labelTypeName')"
                                v-model="form.name"
                                :rules="'required|min:3|max:150'"
                            />
                            <ErrorMessage name="name" class="invalid-feedback d-block" />
                        </div>
                                                    </div>
                            <div class="col-6">
                        <div class="mb-3">
                            <label for="email" class="form-label fw-semibold mb-0">{{ $t("labelEmail") }}</label>
                            <Field
                                name="email"
                                type="email"
                                class="form-control form-control-sm"
                                :placeholder="$t('labelTypeEmail')"
                                v-model="form.email"
                                @blur="validateEmailBackend"
                                :rules="'required|min:5|max:100|email'"
                            />
                            <ErrorMessage name="email" class="invalid-feedback d-block" />
                        </div>
                        </div>
                        </div>
                        <div class="row mb-3">
                            <div class="col-6">
                                <label for="password" class="form-label fw-semibold mb-0">{{ $t("labelPassword") }}</label>
                                <PasswordInputComponent
                                    :placeholder="$t('labelTypePassword')"
                                    :rules="'required|min:6|max:50|custom_password'"
                                    name="password"
                                    v-model="form.password"
                                />                            
                            </div>
                            <div class="col-6">
                                <label for="confirmedPassword" class="form-label fw-semibold mb-0">{{ $t("labelConfirmedPassword") }}</label>
                                <PasswordInputComponent
                                    :placeholder="$t('labelTypeConfirmedPassword')"
                                    :rules="'required|confirmed:password|min:6|max:50'"
                                    name="confirmedPassword"
                                    v-model="form.confirmedPassword"
                                />  
                            </div>
                        </div>                                            
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary btn-sm" @click="close">
                            {{ $t("labelCancel") }}
                        </button>
                        <button type="submit" class="btn btn-primary btn-sm">
                            {{ $t("labelCreateTeamUser") }}
                        </button>
                    </div>                    
                </Form>
            </div>
        </div>
    </div>
    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
    import api from "@/services/api";
    import ErrorCode from "@/constants/Errorcode";
    import ToastAlert from "@/components/common/toast-alert";
    import {Form, Field, ErrorMessage} from "vee-validate";
    import PasswordInputComponent from "@/components/global/PasswordInputComponent.vue";

    export default {
        name: "ModalTeamUser",
        props: {
            teamId: {
                type: Number,
                required: true,
            },
        },
        data() {
            return {
                form: {
                    name: "",
                    email: "",
                    password: "",
                    confirmedPassword: "",
                },
                loading: false,
                validatingEmail: false,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                myInterval: null,
            };
        },
        components: {
            ToastAlert,
            Form, Field, ErrorMessage,
            PasswordInputComponent
        },    
        emits: ["close", "userCreated"],
        methods: {
            async validateEmailBackend() {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(this.form.email.trim())) {
                    return;
                }
                var paramsReq = {
                    email: this.form.email.trim(),
                    userId: null
                };
                let self = this;
                api.post("User/IsEmailInUse", paramsReq )
                    .then(function (response) {
                        if (response && response.data && response.data === true) {
                            self.$refs.form.setFieldError('email', self.$t("labelErrorEmailAlreadyExists"));
                        } else {
                            self.$refs.form.setFieldError('email', "");
                        }
                        self.loading = false;
                    })
                    .catch(function (e) {
                        self.alertToast(self.$t("labelUserError"), "toast-warning");
                        self.loading = false;
                    })
                    .finally(function () {
                        console.log("Finished request.");
                    });
            },
            handleSubmit(e) {

                this.loading = true;
                const user = {
                    name: this.form.name,
                    email: this.form.email,
                    password: this.form.password,
                };
                api.post("User", user)
                    .then((response) => {
                        this.loading = false;
                        this.resetForm();
                        this.$emit("userCreated");
                    })
                    .catch(function (e) {
                        if (e.response && e.response.data && e.response.data.errorCode !== ErrorCode.DefaultError) {
                            switch (e.response.data.errorCode) {
                                case ErrorCode.Duplicated:
                                    this.$refs.form.setFieldError('email', this.$t("labelErrorEmailAlreadyExists"));
                                    break;
                                default:
                                    self.alertToast(this.$t("labelUserError"), "toast-warning");
                            }
                        } else {
                            self.alertToast(this.$t("labelUserError"), "toast-warning");
                        }
                        self.loading = false;
                    })
                    .finally(() => {
                        console.log("Finished request.");
                        this.loading = false;
                    });
            },
            close: function () {
                this.$emit("close");
            },
            resetForm() {
                this.form = {
                    name: "",
                    email: "",
                    teamId: 0,
                };
                this.errors = {};
                this.emailError = "";
                this.nameError = "";
            },
            openModal() {
                this.$refs.modal.style.display = "block";
                this.$refs.modal.classList.add("show");
                document.body.classList.add("modal-open");
            },
            close: function () {
                this.$emit("close");
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                let self = this;
                this.myInterval = setInterval(function () {
                    self.toastMessage = "";
                    self.toastColor = "";
                    self.toastShow = false;
                    clearInterval(self.myInterval);
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval);
                this.myInterval = null;
            },
        },
    };
</script>

<style scoped>
    .custom-header {
        padding: 15px 15px 0;
        border-bottom-width: 0px !important;
    }

    .show {
        display: block;
    }
</style>
