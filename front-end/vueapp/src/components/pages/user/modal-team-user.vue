<template>
    <div class="modal fade show" id="novoUsuarioModal" tabindex="-1" ref="modal">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ $t('labelNewTeam') }}                            
                        <small class="text-muted d-block text-sm">{{ $t('labelNewTeamUserMessage') }}</small>
                    </h6>
                    <button type="button" class="btn-close"  @click="close"></button>
                </div>
                <div class="modal-body">
            
                   <form @submit="handleSubmit">
                        <div class="mb-3">
                            <label for="name" class="form-label">{{ $t('labelName') }}</label>
                            <input
                                name="name"
                                type="text"
                                class="form-control form-control-sm"
                                :placeholder="$t('labelTypeName')"
                                v-model="form.name"
                                @blur="nameError = form.name ? '' : $t('labelRequiredField')"
                                @input="nameError = ''"                                
                            />
                             <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                        </div>
                        <div class="mb-3">
                            <label for="email" class="form-label">{{$t('labelEmail')}}</label>
                            <input
                                name="email"
                                type="email"
                                class="form-control form-control-sm"
                                :placeholder="$t('labelTypeEmail')"
                                v-model="form.email"
                                @blur="validateEmailBackend"
                                @input="emailError = ''"                        
                            />
                             <div v-if="emailError" class="invalid-feedback d-block">{{ emailError }}</div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary btn-sm" @click="close">{{ $t('labelCancel') }}</button>
                            <button type="submit" class="btn btn-primary btn-sm">{{ $t('labelCreateTeamUser') }}</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
import api from "@/services/api";
import ErrorCode from '@/constants/Errorcode';
import ToastAlert from '@/components/common/toast-alert';

export default {
    name: 'ModalTeamUser',
    data() {
        return {
            form: {
                name: '',
                email: ''
            },
            nameError: '',
            emailError: '',
            loading: false,
            validatingEmail: false,
            toastShow: false,
            toastColor: "",
            toastMessage: "",
            myInterval: null,
        }
    },
    components: {
        ToastAlert
    },
    emits: ['close', 'userCreated'],
    methods: {
        async validateEmailBackend() {
            if (!this.form.email || this.form.email.trim() === '') {
                this.emailError =  this.$t('labelRequiredField');
                return;
            }
            var paramsReq = {
                email: this.form.email.trim()
            };
            let self = this;
            api.get('User/EmailExists', { params: paramsReq })
                .then(function (response) {
                    self.loading = false;
                }).catch(function (e) {
                    if (e.response && e.response.data && e.response.data.errorCode !== ErrorCode.DefaultError) {
                        switch (e.response.data.errorCode) {
                            case ErrorCode.Duplicated:
                                self.emailError = self.$t('labelErrorEmailAlreadyExists');
                                break;
                            default:
                                self.alertToast(self.$t('labelUserError'), "toast-warning");
                        }
                    }
                    else {
                        self.alertToast(self.$t('labelUserError'), "toast-warning");
                    }
                    self.loading = false;
                }).finally(function () {
                    console.log("Finished request.");
                });  
        },
        handleSubmit: function (e) {
            e.preventDefault();
            if (!this.form.name || this.form.name.trim() === '') {
                this.nameError =  this.$t('labelRequiredField');
                return;
            }
            if (!this.form.email || this.form.email.trim() === '') {
                this.emailError =  this.$t('labelRequiredField');
                return;
            }

            this.loading = true;
            let self = this;
            api.post('User', {
                    name: this.form.name.trim(),
                    email: this.form.email.trim()
                }).then(function (response) {
                    this.loading = false;
                    this.resetForm();
                    this.$emit('userCreated');
                }).catch(function (e) {
                    self.alertToast(this.$t('labelUserError'), "toast-warning");
                }).finally(function () {
                    console.log("Finished request.");
                    this.loading = false;
                });
        },
        resetForm() {
            this.form = {
                name: '',
                email: ''
            };
            this.errors = {};
            this.emailError = '';
            this.nameError = '';
        },
        openModal() {
            this.$refs.modal.style.display = 'block';
            this.$refs.modal.classList.add('show');
            document.body.classList.add('modal-open');
        },
        close: function () {
            this.$emit('close');
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
    }
}
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