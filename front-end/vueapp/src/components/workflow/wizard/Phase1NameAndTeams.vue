<template>
    <div class="phase-container">
        <div class="row">
            <div class="col">
                <p class="section-title">{{ $t("workflow.basicInfo") }}</p>
            </div>
        </div>
        <div class="row">
            <div class="col-12">
                <label>{{ $t("workflow.name") }}</label>
                <Field name="name"
                       rules="required"
                       v-slot="{ field, errorMessage }"
                       v-model="workflowData.name">
                    <input class="form-control form-control-sm"
                           :placeholder="$t('workflow.namePlaceholder')"
                           v-bind="field"
                           :value="workflowData.name" />
                    <span class="validation-message text-danger" v-if="errorMessage">{{ errorMessage }}</span>
                </Field>
            </div>
        </div>
        <div class="row mt-4">
            <div class="col-12">
                <label>{{ $t("workflow.associatedTeams") }}</label>
                <div v-if="isLoadingTeams">
                    <div class="d-flex justify-content-center">
                        <div class="spinner-border text-primary" role="status"></div>
                    </div>
                </div>
                <div v-else>
                    <Field name="selectedTeams"
                           rules="requiredArray"
                           v-model="selectedTeams"
                           v-slot="{ errors }">
                        <div class="row">
                            <div v-for="team in teamsList"
                                 :key="team.id"
                                 class="col-3 p-1">
                                <div class="form-check d-flex align-items-center">
                                    <input class="form-check-input me-3"
                                           type="checkbox"
                                           :id="`team-${team.id}`"
                                           :value="team.id"
                                           v-model="selectedTeams" />
                                    <label class="form-check-label fw-semibold"
                                           :for="`team-${team.id}`">
                                        {{ team.text }}
                                    </label>
                                </div>
                            </div>
                        </div>
                        <span class="validation-message text-danger" v-if="errors.length">
                            {{ errors[0] }}
                        </span>
                    </Field>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { Field } from "vee-validate";
    import TeamsService from "@/services/teams/TeamsService";

    export default {
        name: "Phase1NameAndTeams",
        components: {
            Field,
        },
        props: {
            initialData: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                workflowData: {
                    name: this.initialData?.name || ""
                },
                selectedTeams: this.initialData?.teams || [],
                teamsList: [],
                isLoadingTeams: true,
            };
        },
        methods: {
            getTeams() {
                this.isLoadingTeams = true;
                TeamsService.getTeamList()
                    .then((response) => {
                        if (response.error !== undefined) return;
                        this.teamsList = response.map(r => ({ id: r.id, text: r.name }));
                    })
                    .finally(() => {
                        this.isLoadingTeams = false;
                    });
            },
            getData() {
                return {
                    name: this.workflowData.name,
                    teams: this.selectedTeams
                };
            }
        },
        created() {
            this.getTeams();

        },
        watch: {
            initialData: {
                handler(newVal) {
                    if (newVal) {
                        this.workflowData.name = newVal.name ?? '';
                        this.selectedTeams = newVal.teams ?? [];
                    } else {
                        this.workflowData.name = '';
                        this.selectedTeams = [];
                    }
                },
                deep: true,
                immediate: true
            }
        }
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
</style>
