<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-6">
                    <div class="row">
                        <div class="col-1">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="returnToTable">
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t(formTitle) }}</h5>
                                <p><small class="text-muted">{{ $t(formSubtitle) }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>            
                <div class="col-auto ms-auto">
                    <button class="btn btn-primary btn-sm" @click="save">
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("labelSave") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <label>{{ $t("labelName") }}</label>
                    <input
                        v-model="profileData.name"
                        class="form-control form-control-sm"
                        @blur="nameError = profileData.name ? '' : $t('labelRequiredField')"
                        @input="nameError = ''"
                    />
                    <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <label class="form-label mb-0">{{ $t("labelPermissions") }}</label>
                            <span class="text-muted">{{ selectedPermissions.length }} {{ $t("labelSelectedWithO") }}</span>
                        </div>

                        <div class="mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                                <input
                                    type="text"
                                    class="form-control form-control-sm"
                                    :placeholder="$t('labelSearchPermissions')"
                                    v-model="searchTerm"
                                />
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
                            <div v-for="permission in filteredPermissions" :key="permission.id" class="p-1">
                                <div class="form-check d-flex align-items-center">
                                    <input
                                        class="form-check-input me-3"
                                        type="checkbox"
                                        :id="`permission-${permission.id}`"
                                        :value="permission.id"
                                        v-model="selectedPermissions"
                                    />
                                    <label
                                        class="form-check-label d-flex align-items-center w-100"
                                        :for="`permission-${permission.id}`"
                                    >
                                        <div>
                                            <div class="fw-semibold">{{ permission.description }}</div>
                                        </div>
                                    </label>
                                </div>
                            </div>
                            <div v-if="permissionError" class="invalid-feedback d-block">{{ permissionError }}</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import { Form, Field, ErrorMessage } from "vee-validate";
    import SelectionListComponent from "@/components/global/SelectionListComponent.vue";
    import PermissionsService from "@/services/permissions/PermissionsService";
    import ProfilesService from "@/services/profiles/ProfilesService";

    export default {
        name: "ProfilesForm",
        components: {
            Form,
            Field,
            ErrorMessage,
            SelectionListComponent,
        },
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            id: {
                type: Number,
                required: false,
                default: null,
            }
        },
        data: () => ({
            profileData: {
                id: "",
                name: "",
                permissions: [],
            },
            isLoading: false,
            permissionsList: [],
            selectedPermissions: [],
            nameError: "",
            permissionError: "",
            searchTerm: "",
        }),
        computed: {
            formTitle() {
                return this.isEdit ? "management.profiles.editTitle" : "management.profiles.createTitle";
            },
            formSubtitle() {
                return this.isEdit ? "management.profiles.editSubtitle" : "management.profiles.createSubtitle";
            },
            filteredPermissions() {
                if (!this.searchTerm) {
                    return this.permissionsList;
                }
                return this.permissionsList.filter((permission) =>
                    permission.name.toLowerCase().includes(this.searchTerm.toLowerCase())
                );
            },
        },
        mounted() {
            this.getPermissions();
        },
        methods: {
            returnToTable() {
                this.$router.push({
                    name: "Management",
                    query: "profiles",
                });
            },
            getPermissions() {
                PermissionsService.getPermissions()
                    .then((response) => {
                        this.permissionsList = response.permissions;
                    });
            },
            validateForm() {
                let valid = true;
                if (!this.profileData.name || this.profileData.name.length < 2) {
                    this.nameError = this.$t("labelRequiredField");
                    valid = false;
                } else if (this.selectedPermissions.length == 0) {
                    this.permissionError = this.$t("labelRequiredField");
                    valid = false;
                }
                return valid;
            },
            selectAll() {
                this.selectedPermissions = this.filteredPermissions.map((user) => user.id);
            },
            clearSelection() {
                this.selectedPermissions = [];
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
                            this.resetData();
                            return this.$notify({
                                title: "Profiles",
                                message: this.$t("labelProfileAddSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Profiles",
                                message: this.$t("labelProfileAddError"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
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
                            return this.$notify({
                                title: "Profiles",
                                message: this.$t("labelProfileEditSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Profiles",
                                message: this.$t("labelProfileEditError"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
            resetForm() {
                this.profileData = { 
                    id: "", 
                    name: "", 
                    permissions: [] 
                };
            },
        },
    };
</script>

<style scoped>
    .container-fluid {
        padding: 0 13px;
    }

    .main-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        background: white;
        padding: 20px 24px;
    }
</style>