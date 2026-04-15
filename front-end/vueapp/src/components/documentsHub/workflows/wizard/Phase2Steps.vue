<template>
    <div class="phase-container">
        <ModalComponent
            id="step-documents-modal"
            ref="stepDocumentsModal"
            title="workflow.stepHasDocumentsTitle"
        >
            <p>{{ $t("workflow.stepHasDocumentsMessage") }}</p>
            <template #footer>
                <div class="modal-footer justify-content-center">
                    <button
                        type="button"
                        class="btn btn-secondary"
                        data-bs-dismiss="modal"
                    >
                        {{ $t("common.close") }}
                    </button>
                </div>
            </template>
        </ModalComponent>
        <div class="row">
            <div class="col">
                <p class="section-title">
                    {{ $t("workflow.stepsTitle") }}
                </p>
            </div>
            <div class="col-auto">
                <button
                    class="btn btn-primary btn-sm"
                    type="button"
                    @click="addStep"
                >
                    <LucideIcon
                        icon="Plus"
                        :size="15"
                    />
                    {{ $t("workflow.createNewStep") }}
                </button>
            </div>
        </div>
        <div v-if="isLoadingProfiles || isLoadingStatus">
            <div class="d-flex justify-content-center">
                <div
                    class="spinner-border text-primary"
                    role="status"
                ></div>
            </div>
        </div>
        <div
            v-else
            class="row"
        >
            <div class="d-flex gap-3 flex-nowrap pb-2">
                <div
                    v-for="step in activeStepsList"
                    :key="step.id ? `id-${step.id}` : `tmp-${step.tempId}`"
                    class="step-card card shadow-sm rounded-3"
                >
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <div class="step-number">
                                {{ step.order }}
                            </div>
                            <!-- NAME -->
                            <Field
                                v-model="step.name"
                                :name="`steps[${step.tempId}].name`"
                                rules="required"
                                v-slot="{ field, errors }"
                            >
                                <div class="d-flex flex-column">
                                    <input
                                        type="text"
                                        class="input-title"
                                        v-bind="field"
                                        v-model="step.name"
                                        :placeholder="$t('workflow.stepNamePlaceholder')"
                                    />
                                    <span
                                        v-if="errors[0]"
                                        class="validation-message text-danger mt-1"
                                    >
                                        {{ errors[0] }}
                                    </span>
                                </div>
                            </Field>
                        </div>

                        <button
                            type="button"
                            class="btn btn-link btn-sm"
                            @click="removeStep(step)"
                            :disabled="isCheckingDocuments"
                        >
                            <LucideIcon icon="X" />
                        </button>
                    </div>

                    <div class="card-body">
                        <!-- STATUS -->
                        <div class="mb-3">
                            <label class="form-label text-muted small">
                                {{ $t("common.status") }}
                            </label>

                            <Field
                                v-model="step.statusId"
                                :name="`steps[${step.tempId}}].statusId`"
                                rules="required"
                                v-slot="{ field, errors }"
                            >
                                <div class="d-flex flex-column">
                                    <select
                                        class="form-select form-select-sm"
                                        v-bind="field"
                                        v-model="step.statusId"
                                    >
                                        <option value="">
                                            {{ $t("workflow.selectStatus") }}
                                        </option>

                                        <option
                                            v-for="s in statusList"
                                            :key="s.id"
                                            :value="String(s.id)"
                                        >
                                            {{ s.label ? $t(s.label) : s.name }}
                                        </option>
                                    </select>

                                    <span
                                        v-if="errors[0]"
                                        class="text-danger small mt-1"
                                    >
                                        {{ errors[0] }}
                                    </span>
                                </div>
                            </Field>
                        </div>

                        <!-- PROFILE -->
                        <div class="mb-2">
                            <label class="form-label text-muted small">
                                {{ $t("workflow.profiles") }}
                            </label>

                            <Field
                                v-model="step.profileId"
                                :name="`steps[${step.tempId}].profileId`"
                                rules="required"
                                v-slot="{ errors }"
                            >
                                <div class="d-flex flex-column">
                                    <div class="input-group">
                                        <span class="input-group-text border-end-0">
                                            <LucideIcon
                                                icon="Users"
                                                :size="16"
                                            />
                                        </span>
                                        <div class="dropdown flex-grow-1">
                                            <button
                                                class="btn btn-light border form-select-sm text-start d-flex justify-content-between align-items-center w-100 dropdown-toggle border-start-0 rounded-start-0 pe-1"
                                                type="button"
                                                data-bs-toggle="dropdown"
                                                data-bs-display="static"
                                                aria-expanded="false"
                                            >
                                                <span class="text-truncate profile-label">
                                                    {{ getProfileName(step.profileId) || $t("workflow.selectProfile") }}
                                                </span>
                                                <LucideIcon
                                                    icon="ChevronDown"
                                                    :size="14"
                                                    class="ms-1 text-muted flex-shrink-0"
                                                />
                                            </button>
                                            <ul class="dropdown-menu p-2 profile-dropdown-menu">
                                                <li class="mb-1">
                                                    <div class="input-group input-group-sm">
                                                        <span class="input-group-text p-1 border-end-0">
                                                            <LucideIcon
                                                                icon="Search"
                                                                :size="14"
                                                            />
                                                        </span>
                                                        <input
                                                            v-model="profileSearches[step.tempId]"
                                                            type="text"
                                                            class="form-control form-control-sm border-start-0"
                                                            :placeholder="$t('filters.search')"
                                                            @click.stop=""
                                                        />
                                                    </div>
                                                </li>
                                                <li v-if="!profileSearches[step.tempId]">
                                                    <a
                                                        class="dropdown-item small"
                                                        :class="{ active: !step.profileId }"
                                                        @click="step.profileId = ''"
                                                    >
                                                        {{ $t("workflow.selectProfile") }}
                                                    </a>
                                                </li>
                                                <li
                                                    v-for="p in getFilteredProfiles(step)"
                                                    :key="p.id"
                                                >
                                                    <a
                                                        class="dropdown-item small"
                                                        :class="{ active: String(p.id) === String(step.profileId) }"
                                                        @click="step.profileId = String(p.id)"
                                                    >
                                                        {{ p.text }}
                                                    </a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>

                                    <span
                                        v-if="errors[0]"
                                        class="text-danger small mt-1"
                                    >
                                        {{ errors[0] }}
                                    </span>
                                </div>
                            </Field>
                        </div>
                    </div>
                </div>
                <div
                    class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0"
                    @click="addStep"
                >
                    <div class="icon-circle mb-2">
                        <LucideIcon
                            icon="Plus"
                            :size="16"
                        />
                    </div>
                    <h6 class="fw-semibold mb-1">
                        {{ $t("workflow.addStep") }}
                    </h6>
                    <p class="text-muted small mb-0">
                        {{ $t("workflow.addStepDescription") }}
                    </p>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import { Field } from "vee-validate";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import StatusService from "@/services/status/StatusService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import ModalComponent from "@/components/global/ModalComponent.vue";

    export default {
        name: "Phase2Steps",
        components: {
            Field,
            ModalComponent,
        },
        props: {
            initialSteps: {
                type: Array,
                default: () => [],
            },
        },
        data() {
            return {
                steps:
                    this.initialSteps.length > 0
                        ? this.initialSteps.map((s) => ({
                              ...s,
                              tempId: s.tempId ?? crypto.randomUUID(),
                              isActive: s.isActive !== false,
                          }))
                        : [],
                profilesList: [],
                statusList: [],
                isLoadingProfiles: true,
                isLoadingStatus: true,
                tempStepCounter: 1,
                isCheckingDocuments: false,
                profileSearches: {},
            };
        },

        computed: {
            activeStepsList() {
                return this.steps.filter((s) => s.isActive !== false);
            },
        },

        methods: {
            addStep() {
                this.steps.push({
                    id: 0,
                    tempId: crypto.randomUUID(),
                    name: "",
                    order: this.steps.length + 1,
                    profileId: "",
                    statusId: "",
                    isActive: true,
                });
            },
            reorderList() {
                this.activeStepsList.forEach((step, index) => {
                    step.order = index + 1;
                });
            },

            async removeStep(step) {
                if (step.id > 0) {
                    const workflowId = this.$route.params.id ?? this.$route.params.workflowId;
                    if (workflowId) {
                        this.isCheckingDocuments = true;
                        try {
                            const count = await WorkflowService.countDocuments(workflowId);
                            if (count > 0) {
                                this.$refs.stepDocumentsModal.open();
                                return;
                            }
                        } finally {
                            this.isCheckingDocuments = false;
                        }
                    }
                }
                const idx = this.steps.findIndex(
                    (s) => (s.id && s.id === step.id) || (s.tempId && s.tempId === step.tempId)
                );
                if (idx !== -1) {
                    this.steps[idx].isActive = false;
                    this.reorderList();
                }
            },

            hasRemovedOriginalSteps() {
                return this.steps.some((s) => s.id > 0 && s.isActive === false);
            },

            getData() {
                return {
                    steps: this.steps
                        .filter((s) => s.isActive !== false)
                        .map((step, index) => ({
                            id: step.id || 0,
                            name: step.name,
                            order: index + 1,
                            profileId: step.profileId ? parseInt(step.profileId) : null,
                            statusId: step.statusId ? parseInt(step.statusId) : null,
                            hasStepTools: step.hasStepTools || false,
                            isActive: true,
                        })),
                };
            },
            getFilteredProfiles(step) {
                const search = (this.profileSearches[step.tempId] || "").toLowerCase();
                if (!search) return this.profilesList;
                return this.profilesList.filter((p) =>
                    p.text.toLowerCase().includes(search)
                );
            },
            getProfileName(profileId) {
                if (!profileId) return "";
                const profile = this.profilesList.find(
                    (p) => String(p.id) === String(profileId)
                );
                return profile ? profile.text : "";
            },
            getProfiles() {
                this.isLoadingProfiles = true;
                ProfilesService.getProfilesList()
                    .then((response) => {
                        if (response.error !== undefined) return;
                        this.profilesList = response.map((r) => ({
                            id: r.id,
                            text: r.name,
                        }));
                    })
                    .finally(() => {
                        this.isLoadingProfiles = false;
                    });
            },
            getStatus() {
                this.isLoadingStatus = true;
                StatusService.getStatusForWorkflowSteps()
                    .then((response) => {
                        if (response.error !== undefined) return;
                        this.statusList = response;
                    })
                    .finally(() => {
                        this.isLoadingStatus = false;
                    });
            },
        },
        created() {
            this.getProfiles();
            this.getStatus();
        },
    };
</script>
<style scoped>
    .phase-container {
        padding: 20px 24px;
    }

    .section-title {
        font-size: 14px;
        color: #6c757d;
        margin-bottom: 16px;
    }

    .step-card {
        min-width: 280px;
        flex-shrink: 0;
    }

    .card-header {
        background-color: var(--color-header-card-step) !important;
        padding: 12px 16px;
    }

    .step-number {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 28px;
        height: 28px;
        border-radius: 50%;
        background-color: #2f80ed;
        color: white;
        font-weight: bold;
        margin-right: 8px;
    }

    .input-title {
        border: none;
        background: transparent;
        font-weight: 600;
        padding: 4px;
        color: var(--color-body-content) !important;
    }

    .input-title:focus {
        outline: none;
        border-bottom: 1px solid #2f80ed;
    }

    .add-step-card {
        min-width: 240px;
        flex-shrink: 0;
        border: 2px dashed var(--color-border-subscription-card) !important;
        cursor: pointer;
        min-height: 240px;
        transition: background-color 0.2s;
    }

    .add-step-card:hover {
        background-color: var(--color-hover-transfer) !important;
    }

    .icon-circle {
        width: 32px;
        height: 32px;
        border: 2px dashed var(--color-border-subscription-card) !important;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto;
        color: #6b7280;
    }

    .border-dashed {
        border-style: dashed !important;
    }

    .profile-dropdown-menu {
        min-width: 100%;
        max-height: 240px;
        overflow-y: auto;
        position: absolute;
        top: 100%;
        bottom: auto;
    }

    .profile-label {
        font-size: 0.8rem;
        max-width: calc(100% - 20px);
    }

    .dropdown-toggle::after {
        display: none;
    }
    .border {
        border-color: var(--color-border-form-control) !important;
    }
</style>
