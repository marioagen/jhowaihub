<template>
    <div class="modal fade show" id="novoTimeModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">            
            <div class="modal-content">
                <div class="overlay" :class="{ active: showModalUserTeam }"></div>
                <div class="modal-header custom-header">
                    <h6 class="modal-title" id="novoTimeModalLabel">
                        {{ $t('labelNewUser') }}                            
                        <small class="text-muted d-block text-sm">{{ $t('labelNewUserMessage') }}</small>
                    </h6>
                    <button type="button" class="btn-close"  @click="close"></button>
                </div>
                <div class="modal-body">
                    <form ref="formRef">
                        <div class="mb-3">
                            <label for="userName" class="form-label">{{ $t('labelName')}}</label>
                            <input type="text"
                                   class="form-control form-control-sm"
                                   id="userName"
                                   ref="userNameInput"
                                   autocomplete="off"
                                   name="userName"
                                   :rules="'required|min:2|max:50'"
                                   v-model="userData.name"
                                   :placeholder="$t('labelTypeUserName')"
                                   @blur="nameError = userData.name ? '' : $t('labelRequiredField')"
                                   @input="nameError = ''" />
                            <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                        </div>
                        <div class="mb-3">
                            <label for="userEmail" class="form-label">{{ $t('labelEmail')}}</label>
                            <input type="text"
                                   class="form-control form-control-sm"
                                   id="userEmail"
                                   ref="userEmailInput"
                                   autocomplete="off"
                                   name="userEmail"
                                   :rules="'required|min:2|max:50'"
                                   v-model="userData.email"
                                   :placeholder="$t('labelTypeUserEmail')"
                                   @blur="emailError = userData.email ? '' : $t('labelRequiredField')"
                                   @input="emailError = ''" />
                            <div v-if="emailError" class="invalid-feedback d-block">{{ emailError }}</div>
                        </div>
                        <div class="mb-3">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label mb-0">{{ $t('labelTeams') }}</label>
                                <span class="text-muted">{{ selectedTeams.length }} {{$t('labelSelectedWithO')}}</span>
                            </div>

                            <div class="mb-3">
                                <div class="input-group">
                                    <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                                    <input type="text" class="form-control form-control-sm" :placeholder="$t('labelSearchTeams')" v-model="searchTerm" />
                                </div>
                            </div>
                            <div class="mb-3">
                                <button type="button" class="btn btn-outline-primary btn-sm me-2" @click="selectAll">
                                    <i class="bi bi-check-all"></i> {{ $t('labelSelectAll') }}
                                </button>
                                <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearSelection">
                                    <i class="bi bi-x-circle"></i> {{ $t('labelClearSelection') }}
                                </button>
                            </div>

                            <div class="border rounded p-1 user-list">
                                <div v-if="loading" class="text-center">
                                    <div class="spinner-border text-primary" role="status">
                                        <span class="visually-hidden">{{ $t('labelLoading') }}</span>
                                    </div>
                                </div>
                                <div v-if="!loading" v-for="team in filteredUsers" :key="team.id" class="p-1">
                                    <div class="form-check d-flex align-items-center">
                                        <input class="form-check-input me-3" type="checkbox" :id="`user-${team.id}`" :value="team.id" v-model="selectedTeams">
                                        <label class="form-check-label d-flex align-items-center w-100" :for="`user-${team.id}`">
                                            <div>
                                                <div class="fw-semibold">{{ team.name }}</div>
                                            </div>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="mb-3">
                            <button type="button" class="btn btn-sm btn-outline-primary w-100" @click="addNewTeam"> + {{ $t('labelNewTeam')}}</button>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" @click="close"> {{ $t('labelCancel') }} </button>
                    <button type="button" class="btn btn-primary btn-sm" @click="saveUser" > {{ $t('labelCreate')}} </button>
                </div>
            </div>
        </div>
    </div>
    <modal-user-team v-if="showModalUserTeam" @close="closeModalUserTeam" @teamCreated="teamCreated"></modal-user-team>
    <toast-alert :showToast="toastShow" :colorToast="toastColor" :messageToast="toastMessage" @close="closeToast" />
</template>

<script>
import api from "@/services/api";
import ModalUserTeam from "@/components/user-manager/users/modals/new-team.vue";
import ToastAlert from '@/components/common/toast-alert';
import ErrorCode from '@/constants/Errorcode';

export default {
    name: 'ModalUser',
    components: {
        ModalUserTeam,
        ToastAlert,
        ErrorCode
    },
    props: {
        userEditing: {
            required: true,
            type: Object,
            default: {}
        },
    },
    data() {
        return {
            userData: {
                id: this.userEditing.id ? this.userEditing.id : null,
                name: this.userEditing.name ? this.userEditing.name : '',
                email: this.userEditing.email ? this.userEditing.email : '',
                teams: this.userEditing.teams ? this.userEditing.teams : []
            },
            selectedTeams: this.userEditing.teams ? this.userEditing.teams.map(u => u.id) : [],
            searchTerm: '',
            teams: [],
            loading: false,
            showModalUserTeam: false,
            toastShow: false,
            toastColor: "",
            toastMessage: "",
            myInterval: null,
            nameError: '',
        }
    },
    emits: ['close', 'userCreated'],
    computed: {
        filteredUsers() {
            if (!this.searchTerm) {
                return this.teams
            }
            return this.teams.filter(team =>
                team.name.toLowerCase().includes(this.searchTerm.toLowerCase())
            )
        }
    },
    mounted() {
        this.loadTeams()
    },
    methods: {
        loadTeams() {
            var paramsReq = {
                search: '',
                pageSize: 0,
                page: 1,
                isAscending: this.isAscending
            };

            api.get('/Team/Paged', { params: paramsReq })
                .then((response) => {
                    this.teams = response.data.content;
                    this.loading = false;
                }).catch((e) => {
                    console.log(e);
                    this.loading = false;
                }).finally(() => {
                    console.log("Finished request.");
                    this.loading = false;
                });  
        },
        selectAll() {
            this.selectedTeams = this.filteredUsers.map(user => user.id)
        },
        clearSelection() {
            this.selectedTeams = []
        },
        addNewTeam() {
            this.showModalUserTeam = true;
        },
        saveUser: function (e) {
            e.preventDefault();
            let response; 
            let self = this;

            if (this.userData.id == null) {

                const user = {
                    name: this.userData.name,
                    email: this.userData.email,
                    teamIds: this.selectedTeams,
                }
                response = api.post('User', user)
            }
            else
            {
                const userEdit = {
                    name: this.userData.name,
                    email: this.userData.email,
                    teamIds: this.selectedTeams,
                    id: this.userData.id
                }
                response = api.put('User', userEdit);
            }
            response.then((response) => {       
                }).catch((e) => {
                     self.alertToast(self.$t('labelUserError'), "toast-warning");
                }).finally(function () {
                    console.log("Finished request.");
                    self.$emit('userCreated');
                });             
        },
        resetForm() {
            this.userData.id = 0;
            this.userData.name = '';
            this.selectedTeams = [];
            this.searchTerm = '';
            
            if (this.$refs.formRef) {
                this.$refs.formRef.resetForm();
            }
        },
        close: function () {
            this.$emit('close');
        },
        getInitials(name) {
            if (!name) return '';
            const parts = name.trim().split(' ');
            if (parts.length === 1) {
                const n = parts[0];
                return (n[0] || '').toUpperCase() + (n[n.length - 1] || '').toUpperCase();
            }
            const first = parts[0][0] || '';
            const last = parts[parts.length - 1].slice(-1) || '';
            return (first + last).toUpperCase();
        },
        closeModalUserTeam() {
            this.showModalUserTeam = false;
            this.loadTeams();
        },
        teamCreated() {
            this.loadTeams();
            this.closeModalUserTeam();
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

.initials{
    width: 30px;
    height: 30px;
}

.user-list {
    max-height: 200px;
    min-height: 200px;
    overflow-y: auto;
}

.show {
    display: block;
}

.overlay.active {
    display: block;
    z-index: 1060;
}

.overlay {
    display: none;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, .85);
    position: absolute;
    left: 0;
    top: 0;
    z-index: -1;
}
</style>