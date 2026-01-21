<template>
    <main>
        <div class="container-fluid scroll-area mx-4 mt-4 mb-4">
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
                        {{ $t("common.save") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <label>{{ $t("common.name") }}</label>
                    <input v-model="profileData.name" class="form-control form-control-sm"
                        @blur="nameError = profileData.name ? '' : $t('validation.required')" @input="nameError = ''" />
                    <div v-if="nameError" class="invalid-feedback d-block">{{ nameError }}</div>
                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <label class="form-label mb-0">{{ $t("management.profiles.permissions") }}</label>
                            <span class="text-muted">{{ selectedPermissions.length }} {{ $t("common.selected")
                            }}</span>
                        </div>
                        <div class="mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                                <input type="text" class="form-control form-control-sm"
                                    :placeholder="$t('management.profiles.searchPermissions')" v-model="searchTerm" />
                            </div>
                        </div>
                        <div class="mb-3">
                            <button type="button" class="btn btn-outline-primary btn-sm me-2" @click="selectAll">
                                <LucideIcon icon="CheckCheck" :size="15" />
                                {{ $t("common.selectAll") }}
                            </button>
                            <button type="button" class="btn btn-outline-secondary btn-sm" @click="clearSelection">
                                <LucideIcon icon="CircleX" :size="15" />
                                {{ $t("common.clearSelection") }}
                            </button>
                        </div>
                        <div v-if="isLoadingPermissions">
                            <LoadingComponent />
                        </div>
                        <div v-else class="accordion-wrapper-scroll border rounded p-2 user-list">
                            <div v-for="(group, index) in filteredPermissions" :key="group.group"
                                class="mb-2 border rounded">
                                <div class="d-flex justify-content-between align-items-center p-2 px-3">
                                    <div>
                                        <strong>{{ $t(group.group) }}</strong>
                                        <span class="text-muted ms-1">
                                            ({{ checkedCount(group.permissions) }} / {{ group.permissions.length }})
                                        </span>
                                    </div>

                                    <a @click="toggleCollapse(index)">
                                        <LucideIcon :icon="opened[index] ? 'ChevronUp' : 'ChevronDown'" :size="20" />
                                    </a>
                                </div>

                                <CollapseComponent ref="collapseComponents" :collapseId="`collapse-${index}`">
                                    <div class="p-1">
                                        <div class="row">
                                            <div v-for="permission in group.permissions" :key="permission.id"
                                                class="col-md-3 p-1">
                                                <div class="form-check d-flex align-items-center">
                                                    <input class="form-check-input me-2" type="checkbox"
                                                        :id="`permission-${permission.id}`" :value="permission.id"
                                                        v-model="selectedPermissions" />
                                                    <label class="form-check-label fw-semibold"
                                                        :for="`permission-${permission.id}`">
                                                        {{ $t(permission.description) }}
                                                    </label>
                                                </div>
                                            </div>
                                        </div>
                                        <div v-if="permissionError" class="invalid-feedback d-block">
                                            {{ permissionError }}
                                        </div>
                                    </div>
                                </CollapseComponent>
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
                                        <h6 class="mb-0 fw-bold">{{ $t("management.profiles.permissionsWorkflow") }}
                                        </h6>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div v-if="isLoadingWorkflowPermissions">
                        <LoadingComponent />
                    </div>
                    <div v-else class="accordion-wrapper-scroll border rounded p-3 workflow-list">
                        <div v-for="workflow in workflowList" :key="workflow.id" class="mb-3">
                            <span class="fw-bold mb-2">{{ workflow.name }}</span>
                            <div v-for="step in workflow.steps" :key="step.id" class="ms-3 mb-2">
                                <div class="row">
                                    <div class="col-2">
                                        <span class="fw-semibold">
                                            {{ step.name }}
                                        </span>
                                    </div>
                                    <div class="col-10">
                                        <div class="row ms-2 justify-content-end">
                                            <div v-for="permission in permissionsWorkflowList" :key="permission.id"
                                                class="col-md-3 p-1">
                                                <div class="form-check d-flex align-items-center">
                                                    <input class="form-check-input me-2" type="checkbox"
                                                        :id="`permission-${profileData.id ?? 'new'}-${step.id}-${permission.id}`"
                                                        :value="`${step.id}:${permission.id}`"
                                                        v-model="selectedWorkflowPermissions" />
                                                    <label class="form-check-label"
                                                        :for="`permission-${profileData.id ?? 'new'}-${step.id}-${permission.id}`">
                                                        {{ $t(permission.description) }}
                                                    </label>
                                                </div>
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
    </main>
</template>

<script>
import PermissionsService from "@/services/permissions/PermissionsService";
import ProfilesService from "@/services/profiles/ProfilesService";
import WorkflowService from "@/services/workflow/WorkflowService";
import CollapseComponent from "@/components/global/CollapseComponent.vue";
import LoadingComponent from "@/components/global/LoadingComponent.vue";

export default {
    name: "ProfilesForm",
    components: {
        LoadingComponent,
        CollapseComponent,
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
        isLoadingPermissions: true,
        isLoadingWorkflowPermissions: true,
        workflowList: [],
        permissionsList: [],
        permissionsWorkflowList: [],
        selectedPermissions: [],
        selectedWorkflowPermissions: [],
        nameError: "",
        permissionError: "",
        searchTerm: "",
        opened: {},
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

            const term = this.searchTerm.toLowerCase();
            return this.permissionsList
                .map(group => {
                    const filtered = group.permissions.filter(p =>
                        p.name.toLowerCase().includes(term) ||
                        this.$t(p.description).toLowerCase().includes(term)
                    );

                    return {
                        ...group,
                        permissions: filtered
                    };
                })
                .filter(group => group.permissions.length > 0);
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
            this.isLoadingPermissions = true;
            PermissionsService.getPermissions()
                .then((response) => {
                    this.permissionsList = response.permissions;
                })
                .finally(() => {
                    this.isLoadingPermissions = false;
                });
        },
        getWorkflowPermissions() {
            this.isLoadingWorkflowPermissions = true;
            PermissionsService.getWorkflowPermissions()
                .then((response) => {
                    this.permissionsWorkflowList = response.permissions;
                })
                .finally(() => {
                    this.isLoadingWorkflowPermissions = false;
                });
        },
        getWorkflows() {
            WorkflowService.getWorkflowCompleteList()
                .then((response) => {
                    this.workflowList = response;
                })
        },
        setupEdit() {
            if (!this.isEdit) return;
            ProfilesService.getProfileById(this.id)
                .then((response) => {
                    this.profileData = response;
                    this.selectedPermissions = response.permissions.map(p => p.id);
                    // this.selectedWorkflowPermissions = response.workflowPermission;
                    this.selectedWorkflowPermissions = response.workflowPermission.map(
                        wp => `${wp.stepId}:${wp.permissionId}`
                    );
                });
        },
        validateForm() {
            let valid = true;

            if (!this.profileData.name || this.profileData.name.length < 2) {
                this.nameError = this.$t("validation.required");
                valid = false;
            } else {
                this.nameError = "";
            }

            if (this.selectedPermissions.length === 0) {
                this.permissionError = this.$t("validation.required");
                valid = false;
            } else {
                this.permissionError = "";
            }

            return valid;
        },
        selectAll() {
            this.selectedPermissions = this.filteredPermissions.flatMap((group) => group.permissions.map((permission) => permission.id));
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
                permissionsWorkflow: this.formatWorkflowPermissions(),
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
                            message: this.$t("management.profiles.errors.saveError"),
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
                permissionsWorkflow: this.formatWorkflowPermissions(),
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
                            message: this.$t("management.profiles.errors.editError"),
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                })
                .finally(() => {
                    this.isLoading = false;
                });
        },
        formatWorkflowPermissions() {
            return this.selectedWorkflowPermissions.map(v => {
                const [stepId, permissionId] = v.split(":");
                return {
                    profileId: this.isEdit ? this.profileData.id : null,
                    stepId: parseInt(stepId),
                    permissionId: parseInt(permissionId),
                };
            });
        },
        resetForm() {
            this.profileData = {
                id: "",
                name: "",
                permissions: []
            };
        },
        checkedCount(permissions) {
            return permissions.filter(p => this.selectedPermissions.includes(p.id)).length;
        },
        toggleCollapse(index) {
            const collapse = this.$refs.collapseComponents[index];
            if (collapse && collapse.toggle) {
                collapse.toggle();
                this.$set
                    ? this.$set(this.opened, index, !this.opened[index])
                    : (this.opened[index] = !this.opened[index]);
            }
        }
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

.accordion-wrapper-scroll {
    max-height: 300px;
    overflow-y: auto;
    scrollbar-width: thin;
}

.accordion-wrapper-scroll::-webkit-scrollbar {
    width: 6px;
}

.accordion-wrapper-scroll::-webkit-scrollbar-thumb {
    background-color: rgba(0, 0, 0, 0.2);
    border-radius: 3px;
}
</style>