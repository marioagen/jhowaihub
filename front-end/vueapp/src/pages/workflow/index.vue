<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div
                    class="d-flex justify-content-between align-items-center mb-3"
                >
                    <div>
                        <h5 class="mb-0 fw-bold">
                            {{
                                $t(
                                    "workflow.managementTitle"
                                )
                            }}
                        </h5>
                        <p>
                            <small class="text-muted">
                                {{
                                    $t(
                                        "workflow.managementSubtitle"
                                    )
                                }}
                            </small>
                        </p>
                    </div>
                    <button
                        class="btn btn-primary btn-sm"
                        @click="redirectToForm"
                    >
                        <LucideIcon
                            icon="Plus"
                            :size="17"
                        />
                        {{ $t("workflow.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <WorkflowFilters
                            @filter="filterData"
                            class="ms-auto"
                            :teamsList="teamsList"
                            :usersList="usersList"
                        />
                    </div>
                </div>
                <WorkflowTable ref="WorkflowTable" />
            </div>
        </div>
    </main>
</template>
<script>
    import WorkflowTable from "@/components/workflow/WorkflowTable.vue";
    import WorkflowFilters from "@/components/workflow/WorkflowFilters.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import UsersService from "@/services/users/UserService";
    export default {
        name: "WorkflowManagement",
        data() {
            return {
                teamsList: [],
                usersList: [],
            };
        },
        components: {
            WorkflowFilters,
            WorkflowTable,
        },
        methods: {
            filterData(filters) {
                this.$refs.WorkflowTable.filters = filters;
                this.$refs.WorkflowTable.getWorkflowList();
            },
            redirectToForm() {
                this.$router.push({ name: "NewWorkflow" });
            },
            getTeams() {
                TeamsService.getTeamList().then(
                    (response) => {
                        this.teamsList = response;
                    }
                );
            },
            getUsers() {
                UsersService.getAllUsers().then(
                    (response) => {
                        this.usersList = response;
                    }
                );
            },
        },
        created() {
            this.getTeams();
            this.getUsers();
        },
    };
</script>
<style scoped>
    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    tbody {
        background-color: #fff !important;
    }

    .content-left-middle {
        text-align: left;
        vertical-align: middle;
        max-width: 200px;
    }

    .content-right-middle {
        text-align: right;
        vertical-align: middle;
    }

    .content-center-middle {
        text-align: center;
        vertical-align: middle;
    }

    .bg-success {
        background-color: #edfef2 !important;
        color: #0eaa42 !important;
        font-weight: inherit !important;
        padding: 8px 12px !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>
