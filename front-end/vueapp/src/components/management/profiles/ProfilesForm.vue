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
                                <LucideIcon icon="CheckCheck" :size="15" />
                                {{ $t("labelSelectAll") }}
                            </button>
                            <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearSelection">
                                <LucideIcon icon="CircleX" :size="15" />
                                {{ $t("labelClearSelection") }}
                            </button>
                        </div>
                        <div class="border rounded p-2 user-list">
                            <div class="row ms-2">
                                <div
                                    v-for="permission in filteredPermissions"
                                    :key="permission.id"
                                    class="col-md-3 p-1"
                                >
                                    <div class="form-check d-flex align-items-center">
                                        <input
                                            class="form-check-input me-2"
                                            type="checkbox"
                                            :id="`permission-${permission.id}`"
                                            :value="permission.id"
                                            v-model="selectedPermissions"
                                        />
                                        <label
                                            class="form-check-label fw-semibold"
                                            :for="`permission-${permission.id}`"
                                        >
                                            {{ permission.description }}
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <div v-if="permissionError" class="invalid-feedback d-block">
                                {{ permissionError }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm mt-2">
                    <div class="row align-items-center mb-3">
                        <div class="col-6">
                            <div class="row">
                                <div class="col-1">
                                    <LucideIcon icon="Lock" :size="15" />
                                </div>
                                <div class="col-10">
                                    <div>
                                        <h6 class="mb-0 fw-bold">{{ $t("management.profiles.permissionsWorkflow") }}</h6>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="border rounded p-3 workflow-list">
                        <div
                            v-for="workflow in workflowList"
                            :key="workflow.id"
                            class="mb-3"
                        >
                            <span class="fw-bold mb-2">{{ workflow.name }}</span>
                            <div
                                v-for="step in workflow.steps"
                                :key="step.id"
                                class="ms-3 mb-2"
                            >
                                <div class="row">
                                    <div class="col-2">
                                        <span class="fw-semibold">
                                            {{ step.name }}
                                        </span>
                                    </div>
                                    <div class="col-10">
                                        <div class="row ms-2 justify-content-end">
                                            <div
                                                v-for="permission in permissionsWorkflowList"
                                                :key="permission.id"
                                                class="col-md-3 p-1"
                                            >
                                                <div class="form-check d-flex align-items-center">
                                                    <input
                                                        class="form-check-input me-2"
                                                        type="checkbox"
                                                        :id="`permission-${profileId ?? 'new'}-${step.id}-${permission.id}`"
                                                        :value="{
                                                            profileId: id || null,
                                                            stepId: step.id,
                                                            permissionId: permission.id
                                                        }"
                                                        v-model="selectedWorkflowPermissions"
                                                    />
                                                    <label
                                                        class="form-check-label"
                                                        :for="`permission-${workflow.id}-${step.id}-${permission.id}`"
                                                    >
                                                        {{ permission.description }}
                                                    </label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div v-if="permissionError" class="invalid-feedback d-block">
                            {{ permissionError }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import PermissionsService from "@/services/permissions/PermissionsService";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import WorkflowService from "@/services/workflow/WorkflowService";

    export default {
        name: "ProfilesForm",
        components: {
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
                workflowPermissions: [],
            },
            isLoading: false,
            workflowList: [],
            permissionsList: [],
            permissionsWorkflowList: [],
            selectedPermissions: [],
            selectedWorkflowPermissions: [],
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
            this.getWorkflows();
            this.getPermissions();
            this.getWorkflowPermissions();
            this.setupEdit();
        },
        methods: {
            returnToTable() {
                this.$router.push({
                    name: "Management",
                    query: { tab: "profiles" },
                });
            },
            getPermissions() {
                PermissionsService.getPermissions()
                    .then((response) => {
                        this.permissionsList = response.permissions;
                    });
            },
            getWorkflowPermissions() {
                PermissionsService.getWorkflowPermissions()
                    .then((response) => {
                        this.permissionsWorkflowList = response.permissions;
                    });
            },
            getWorkflows() {
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email)
                    .then((response) => {
                        this.workflowList = response;
                    })
            },
            setupEdit() {
                if(!this.isEdit) return;
                ProfilesService.getProfileById(this.id)
                    .then((response) => {
                        this.profileData = response;
                        this.selectedPermissions = response.permissions.map(p => p.id);
                        this.selectedWorkflowPermissions = response.workflowPermission;
                    });
            },
            validateForm() {
                let valid = true;
                if (!this.profileData.name || this.profileData.name.length < 2) {
                    this.nameError = this.$t("validation.required");
                    valid = false;
                } else if (this.selectedPermissions.length == 0) {
                    this.permissionError = this.$t("validation.required");
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
                    permissionsWorkflow: this.selectedWorkflowPermissions,
                };

                ProfilesService.addProfile(paramsReq)
                    .then((result) => {
                        if (result.success) {
                            this.returnToTable();
                            return this.$notify({
                                title: "Profiles",
                                message: this.$t("management.profiles.saveSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Profiles",
                                message: this.$t("management.profiles.saveError"),
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
                    permissionsWorkflow: this.selectedWorkflowPermissions,
                };
                ProfilesService.updateProfile(paramsReq)
                    .then((result) => {
                        if (result.success) {
                            this.returnToTable();
                            return this.$notify({
                                title: "Profiles",
                                message: this.$t("management.profiles.editSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Profiles",
                                message: this.$t("management.profiles.editError"),
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