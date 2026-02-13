<template>
    <div class="row">
        <div class="col-8">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search"
                                size="16" />
                </span>
                <input id="InputSearch"
                       type="text"
                       class="form-control form-control-sm border-start-0 custom-input"
                       :class="{
                        'border-end-0': showCleanBtn,
                    }"
                       v-model="filters.input"
                       @keydown.enter="filterData"
                       @keydown.delete="filterData"
                       :placeholder="
                        $t('filters.documentInput')
                    "
                       ref="searchInpt" />
                <span v-if="showCleanBtn"
                      class="input-group-text border-start-0 bg-white"
                      @click="cleanInput">
                    <LucideIcon icon="X"
                                :size="16" />
                </span>
            </div>
        </div>
        <div class="col-2">
            <select v-model="filters.statusId"
                    class="form-select form-select-sm w-100"
                    @change="filterData">
                <option value="">
                    {{ $t("filters.statusSelect.none") }}
                </option>
                <option v-for="status in statusList"
                        :key="status.id"
                        :value="status.id">
                    {{ $t("workflow.statusList." + status.name?.toLowerCase()) || status.name }}
                </option>
            </select>
        </div>
        <div class="col-3 doc-filters-select-col">
            <select v-model="filters.workflowId"
                    class="form-select form-select-sm w-100"
                    @change="filterData">
                <option value="">
                    {{ $t("filters.workflowSelect.none") }}
                </option>
                <option :value="0">
                    {{
                        $t(
                            "filters.workflowSelect.withWorkflow"
                        )
                    }}
                </option>
                <option v-for="workflow in workflowsList"
                        :key="workflow.id"
                        :value="workflow.id">
                    {{ workflow.name }}
                </option>
            </select>
        </div>
        <div class="col-1 doc-filters-btn-col">
            <button v-tooltip="
                    filters.isAllUsers
                        ? $t(
                              'filters.assignment.currentUser'
                          )
                        : $t('filters.assignment.allUsers')
                "
                    class="btn table-btn btn-sm"
                    :class="
                    filters.isAllUsers
                        ? 'btn-outline-secondary'
                        : 'btn-outline-primary'
                "
                    type="button"
                    style="
                    display: flex;
                    align-items: center;
                    justify-content: center;
                "
                    @click="filterUsers">
                <LucideIcon icon="User" />
            </button>
        </div>
    </div>
</template>
<script>
    export default {
        name: "DocumentFilters",
        props: {
            workflowsList: {
                type: [Object, Array],
                required: true,
            },
            statusList: {
                type: Array,
                default: () => [],
            },
        },
        data() {
            return {
                filters: {
                    input: "",
                    workflowId: "",
                    workflows: [],
                    isAllUsers: true,
                    login: this.$store.state.userProfile
                        .login,
                    colType: 2,
                    statusId: "",
                },
            };
        },
        watch: {
            workflowsList: {
                immediate: true,
                handler(newVal) {
                    if (newVal.length) {
                        this.$emit("filter", {
                            ...this.filters,
                        });
                    }
                },
            },
        },
        methods: {
            filterData() {
                switch (this.filters.workflowId) {
                    case "":
                        this.filters.workflows = [];
                        break;
                    case 0:
                        this.filters.workflows =
                            this.workflowsList.map(
                                (t) => t.id
                            );
                        break;
                    default:
                        this.filters.workflows = [
                            this.filters.workflowId,
                        ];
                }

                this.$emit("filter", { ...this.filters });
            },
            filterUsers() {
                this.filters.isAllUsers =
                    !this.filters.isAllUsers;
                this.filterData();
            },
            cleanInput() {
                this.filters.input = "";
                this.filterData();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== "";
            },
        },
    };
</script>
<style scoped>
    .custom-input {
        font-size: 12px;
    }

        .custom-input::placeholder {
            font-size: 12px;
            color: #999;
        }

    .doc-filters-select-col {
        min-width: 0;
    }

    .doc-filters-btn-col {
        flex-shrink: 0;
        min-width: 2.25rem;
    }
</style>
