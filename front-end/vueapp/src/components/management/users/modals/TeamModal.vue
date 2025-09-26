<template>
    <div class="modal fade show" id="novoUsuarioModal" tabindex="-1" ref="modal">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ $t("labelNewTeam") }}
                        <small class="text-muted d-block text-sm">{{ $t("labelNewTeamMessage") }}</small>
                    </h6>
                    <button type="button" class="btn-close" @click="close"></button>
                </div>
                <Form @submit="handleSubmit">
                    <div class="modal-body">
                        <div class="mb-3">
                            <label for="name" class="form-label fw-semibold mb-0">{{ $t("labelName") }}</label>
                            <Field
                                name="name"
                                type="text"
                                class="form-control form-control-sm"
                                :placeholder="$t('labelTypeTeamName')"
                                v-model="form.name"
                                :rules="'required|min:3|max:100'"
                            />
                            <ErrorMessage name="name" class="invalid-feedback d-block" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="close">
                            {{ $t("labelCancel") }}
                        </button>
                        <button type="submit" class="btn btn-primary btn-sm">{{ $t("labelCreate") }}</button>
                    </div>
                </Form>
            </div>
        </div>
    </div>
    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
    import api from "@/services/api";
    import { Form, Field, ErrorMessage } from "vee-validate";
    import ToastAlert from "@/components/common/toast-alert";

    export default {
        name: "ModalUserTeam",
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
            Form,
            Field,
            ErrorMessage,
        },
        emits: ["close", "teamCreated"],
        methods: {
            handleSubmit(e) {
                this.loading = true;
                let team = {
                    id: 0,
                    name: this.form.name.trim(),
                    users: [],
                };
                api.post("Team", team)
                    .then((response) => {
                        this.loading = false;
                        this.resetForm();
                        this.$emit("teamCreated");
                    })
                    .catch((e) => {
                        this.alertToast(this.$t("labelTeamError"), "toast-warning");
                    })
                    .finally(() => {
                        console.log("Finished request.");
                        this.loading = false;
                        this.close();
                    });
            },
            resetForm() {
                this.form = {
                    name: "",
                    userId: 1,
                };
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
        created() {
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
