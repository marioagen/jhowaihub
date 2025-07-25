<template>
    <ModalComponent id="profilesModal"
                    :isLoading="isLoading"
                    @save="save"
                    ref="ProfilesModal">
        <template #header>
            <div class="modal-header">
                <h6 class="modal-title">
                    {{ $t(titleText) }}
                    <small class="text-muted d-block text-sm">{{ $t(subTitleText) }}</small>
                </h6>
                <button class="btn-close"
                        data-bs-dismiss="modal"
                        @click="close" />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <label>{{$t("labelName")}}</label>
                <input v-model="profileData.name" class="form-control"
                       @blur="nameError = profileData.name ? '' : $t('labelRequiredField')"
                       @input="nameError = ''" />
                <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <label class="form-label mb-0">{{ $t("labelPermissions") }}</label>
                        <span class="text-muted">
                            {{ selectedPermissions.length }} {{ $t("labelSelectedWithO") }}
                        </span>
                    </div>

                    <div class="mb-3">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                            <input type="text"
                                   class="form-control form-control-sm"
                                   :placeholder="$t('labelSearchPermissions')"
                                   v-model="searchTerm" />
                        </div>
                    </div>
                    <div class="mb-3">
                        <button type="button" class="btn btn-outline-primary btn-sm me-2" @click="selectAll">
                            <i class="bi bi-check-all"></i>
                            {{ $t("labelSelectAll") }}
                        </button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearSelection">
                            <i class="bi bi-x-circle"></i>
                            {{ $t("labelClearSelection") }}
                        </button>
                    </div>

                    <div class="border rounded p-1 user-list">
                        <div v-if="loading" class="text-center">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">{{ $t("labelLoading") }}</span>
                            </div>
                        </div>
                        <div v-if="!loading" v-for="permission in filteredPermissions" :key="permission.id" class="p-1">
                            <div class="form-check d-flex align-items-center">
                                <input class="form-check-input me-3"
                                       type="checkbox"
                                       :id="`permission-${permission.id}`"
                                       :value="permission.id"
                                       v-model="selectedPermissions" />
                                <label class="form-check-label d-flex align-items-center w-100"
                                       :for="`permission-${permission.id}`">
                                    <div>
                                        <div class="fw-semibold">{{ permission.name }}</div>
                                    </div>
                                </label>
                            </div>
                        </div>
                        <div v-if="permissionError" class="invalid-feedback d-block">{{ permissionError }}</div>
                    </div>
                </div>
            </div>
        </template>

        <template #footer>
            <div class="modal-footer">
                <button class="btn btn-secondary btn-sm"
                        @click="close">
                    {{ $t("labelCancel") }}
                </button>
                <button class="btn btn-primary btn-sm"
                        @click="save">
                    {{ $t(saveText) }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
    import ModalComponent from '@/components/global/ModalComponent.vue';
    import PermissionsService from '@/services/permissions/PermissionsService';
    import ProfilesService from '@/services/profiles/ProfilesService';

    export default {
        components: {
            ModalComponent
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data: () => ({
            profileData: {
                id: "",
                name: "",
                permissions: [],
            },
            isLoading: false,
            permissions: [],
            selectedPermissions: [],
            nameError: "",
            permissionError: "",
            searchTerm: "",
        }),
        computed: {
            titleText() {
                return this.isEdit ? "labelEditTitleProfile" : "labelSaveTitleProfile";
            },
            subTitleText() {
                return this.isEdit ? "labelEditSubTitleProfile" : "labelSaveSubTitleProfile";
            },
            saveText() {
                return this.isEdit ? "labelEditProfile" : "labelCreateProfile";
            },
            filteredPermissions() {
                if (!this.searchTerm) {
                    return this.permissions;
                }
                return this.permissions.filter(
                    (permission) =>
                        permission.name.toLowerCase().includes(this.searchTerm.toLowerCase())
                );
            },
        },
        methods: {
            validateForm() {
                let valid = true;
                if (!this.profileData.name || this.profileData.name.length < 2) {
                    this.nameError = this.$t("labelRequiredField");
                    valid = false;
                }
                else if (this.selectedPermissions.length == 0) {
                    this.clearMyInterval();
                    this.alertToast(this.$t("labelNoPermissionChosen") + ".", "toast-warning");
                    valid = false;
                }
                return valid;
            },
            open(profile = null) {
                if (profile === null) {
                    this.resetData();
                    this.selectedPermissions = [];
                } else {
                    this.profileData = profile;
                    this.selectedPermissions = profile.permissions ? profile.permissions.map((u) => u.id) : [];
                }
                this.$refs.ProfilesModal.open();
            },
            close() {
                this.$refs.ProfilesModal.close();
            },
            resetData() {
                this.profileData = { id: "", name: "",  permissions: [] };
            },
            save() {
                if (this.isEdit) {
                    return this.editProfile();
                }
                return this.createProfile();
            },
            createProfile() {
                this.isLoading = true;
                if (!this.validateForm()) return;
                var paramsReq = {
                    name: this.profileData.name,
                    permissionsIds: this.selectedPermissions,
                };
                ProfilesService.addProfile(paramsReq)
                    .then((result) => {
                        if (result.success) {
                            this.alertToast(this.$t("labelDocumentTypeSuccess"), "toast-success");
                            this.close();
                            this.resetData();
                            this.$emit("reload");
                            return;
                        }
                        const messageKey = result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";
                        this.alertToast(this.$t(messageKey), "toast-warning");
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });

            },
            editProfile() {
                this.isLoading = true;
                if (!this.validateForm()) return;
                var paramsReq = {
                    id: this.profileData.id,
                    name: this.profileData.name,
                    permissionsIds: this.selectedPermissions,
                };
                ProfilesService.updateProfile(paramsReq)
                    .then((result) => {
                        if (result.success) {
                            this.alertToast(this.$t("labelDocumentTypeEditSuccess"), "toast-success");
                            this.close();
                            this.$emit("reload");
                            return;
                        }

                        const messageKey = result.status === 409 ? "labelDocumentTypeAlreadyExists" : "labelDocumentTypeError";
                        this.alertToast(this.$t(messageKey), "toast-warning");
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            alertToast(msg, color) {
                this.clearMyInterval();
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;

                this.myInterval = setTimeout(() => {
                    this.toastMessage = "";
                    this.toastColor = "";
                    this.toastShow = false;
                    this.myInterval = null;
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval() {
                if (this.myInterval) {
                    clearTimeout(this.myInterval);
                    this.myInterval = null;
                }
            },
            selectAll() {
                this.selectedPermissions = this.filteredPermissions.map((user) => user.id);
            },
            clearSelection() {
                this.selectedPermissions = [];
            },
            getPermissions(obj) {
                PermissionsService.getPermissions()
                    .then((response) => {
                        this.permissions = response.permissions;
                    })
                    .finally(() => {
                    });
            },
        },
        created() {
            this.getPermissions();
        },
    }
</script>