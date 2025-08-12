<template>
    <main>
        <FullscreenLoadingComponent v-if="isLoading" />
        <div 
            v-else 
            class="container-fluid scroll-area mx-4 mt-4"
        >
            <div class="row align-items-center">
                <div class="col-auto">
                    <div>
                         <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                         <p><small class="text-muted">{{ $t("workflow.subtitle") }}</small></p>
                     </div>
                </div>

                <div class="col-auto ms-auto">
                    <button 
                        class="btn btn-primary btn-sm" 
                        :disabled="canSave"
                        @click="save"
                    >
                        <LucideIcon icon="Save" size="15" />
                        {{ $t("quizzes.formSave") }}
                    </button>
                </div>
            </div>
            
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <div class="row">
                        <div class="col">
                            <p><small class="text-muted">{{ $t("workflow.manage") }}</small></p>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col">
                            <label>{{ $t("workflow.name") }}</label>
                            <input 
                                class="form-control form-control-sm"
                                :placeholder="$t('workflow.name')"
                                v-model="workflowData.name"
                            />
                        </div>
                        <div class="col">
                            <label>{{ $t("workflow.responsableTeam") }}</label>
                            <div class="input-group">
                                <span class="input-group-text border-end-0 bg-white">
                                    <LucideIcon icon="Users" size="16" />
                                </span>
                                <select
                                    id="typeDocId"
                                    class="form-select form-select-sm border-start-0"
                                    v-model="workflowData.team"
                                >
                                    <option value="">{{ $t("workflow.responsableTeam") }}</option>
                                    <option 
                                        v-for="(item, index) in teamsList"
                                        :key="index"
                                        :value="item.id" 
                                    >
                                        {{ item.id }} - {{ item.text }}
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row mt-4">
                <div class="row d-flex justify-content-between align-items-center">
                    <div class="col-auto">
                        <h6 class="mb-4">{{ $t("workflow.steps") }}</h6>
                    </div>
                    <div class="col-auto">
                        <button class="btn btn-primary btn-sm" @click="addStep">
                            <LucideIcon icon="Plus" size="15" />
                            {{ $t("workflow.createNewStep") }}
                        </button>
                    </div>
                </div>
                <div class="row">
                    <div class="d-flex gap-3 overflow-auto flex-nowrap pb-2">
                        <WorkflowStepComponent
                            v-for="(step, index) in stepsList"
                            :key="index"
                            :step="step"
                            :index="index + 1"
                            :is-last="index === stepsList.length - 1" 
                            :profilesList="profilesList"
                            :statusList="statusList"
                            @update-step="updateStep(index, $event)"
                            @remove-step="removeStep(index)"
                            class="workflow-step-card"
                        />
                        
                        <div class="add-step-card text-center p-4 rounded-3 border-dashed flex-shrink-0" @click="addStep">
                            <div class="icon-circle mb-2">
                                <LucideIcon icon="Plus" size="16" />
                            </div>
                            <h6 class="fw-semibold mb-1">{{ $t("workflow.addBtn") }}</h6>
                            <p class="text-muted small mb-0">{{ $t("workflow.addBtnDescription") }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import WorkflowStepComponent from "@/components/workflow/WorkflowStepComponent.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import StatusService from "@/services/status/StatusService";
    import ProfilesService from "@/services/profiles/ProfilesService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    export default {
        name: "EditBoard",
        components: {
            FullscreenLoadingComponent,
            WorkflowStepComponent,
        },
        data() {
            return {
                profilesList: [],
                statusList: [],
                teamsList: [],
                stepsList: [],
                steps: {
                    status: "",
                    profile: "",
                },
                workflowData: {
                    name: "",
                    team: "",
                },
                isLoading: false,
            };
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setCrumbsData();
            },
        },
        computed: {
            canSave() {
                return !this.stepsList.length > 0;
            }
        },
        methods: {
            getTeams() {
                TeamsService.getTeamList()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        for (let i = 0; i < response.length; i++) {
                            var item = {
                                id: response[i].id,
                                text: response[i].name,
                            };
                            this.teamsList.push(item);
                        }
                    });
            },
            getStatus() {
                StatusService.getStatus()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        this.statusList = response;
                    });
            },
            getProfiles() {
                ProfilesService.getProfilesList()
                    .then((response) => {
                        if(response.error !== undefined) return;
                        for (let i = 0; i < response.length; i++) {
                            var item = {
                                id: response[i].id,
                                text: response[i].name,
                            };
                            this.profilesList.push(item);
                        }
                    });
            },
            updateStep(index, updatedStep) {
                this.stepsList[index] = { ...this.stepsList[index], ...updatedStep };
            },
            addStep() {
                this.stepsList.push({
                    status: '',
                    profile: '',
                });
            },
            removeStep(index) {
                this.stepsList.splice(index, 1);
            },
            save() {
                this.isLoading = true;
                let params = {
                    name: this.workflowData.name,
                    teamId: this.workflowData.team,
                    steps: this.stepsList
                };
                console.log(params)
                WorkflowService.createWorkflow(params)
                    .then((response) => {
                        if(response) {
                            //redict somewhere
                            return this.$notify({
                                title: 'Workflow',
                                message: 'workflow.createSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'Workflow',
                            message: 'workflow.createError',
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
        created() {
            this.getTeams();
            this.getStatus();
            this.getProfiles();
        },  
    };
</script>

<style scoped>
    @import "@vueform/multiselect/themes/default.css";

    .multiselect-dropdown {
        max-height: var(--ms-max-height) !important;
    }

    .form-save {
        padding-top: 20px !important;
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .main-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        background: white;
        padding: 20px 24px;
    }

    .add-step-card {
        border: 2px dashed #d1d5db;
        cursor: pointer;
        min-height: 240px;
        transition: background-color 0.2s;
    }

    .add-step-card:hover {
        background-color: #f9fafb;
    }

    .icon-circle {
        width: 32px;
        height: 32px;
        border: 1.5px dashed #9ca3af;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto;
        color: #6b7280;
    }
    .workflow-step-card {
        min-width: 280px;
        flex-shrink: 0;
    }

    .add-step-card {
        min-width: 240px;
        flex-shrink: 0;
        border: 2px dashed #d1d5db;
        cursor: pointer;
        min-height: 240px;
        transition: background-color 0.2s;
    }

    .add-step-card:hover {
        background-color: #f9fafb;
    }
</style>
