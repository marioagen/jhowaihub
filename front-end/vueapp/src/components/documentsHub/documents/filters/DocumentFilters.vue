<template>
    <div class="row p-0">
        <div class="col-4">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon
                        icon="Search"
                        size="16"
                    />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{
                        'border-end-0': showCleanBtn,
                    }"
                    v-model="filters.input"
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.documentInput')"
                    ref="searchInpt"
                />
                <span
                    v-if="showCleanBtn"
                    class="input-group-text border-start-0"
                    @click="cleanInput"
                >
                    <LucideIcon
                        icon="X"
                        :size="16"
                    />
                </span>
            </div>
        </div>
        <div class="col-2">
            <select
                v-model="filters.statusId"
                class="form-select form-select-sm w-100"
                @change="filterData"
            >
                <option value="">
                    {{ $t("filters.statusSelect.none") }}
                </option>
                <option
                    v-for="status in statusList"
                    :key="status.id"
                    :value="status.id"
                >
                    {{ $t("workflow.statusList." + status.name?.toLowerCase()) || status.name }}
                </option>
            </select>
        </div>
        <div class="col-3">
            <div class="dropdown workflow-filter-dropdown">
                <button
                    class="btn btn-light border form-select-sm text-start d-flex justify-content-between align-items-center w-100 dropdown-toggle pe-1"
                    type="button"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                >
                    <span class="text-truncate workflow-filter-label">{{ selectedWorkflowLabel }}</span>
                    <LucideIcon
                        icon="ChevronDown"
                        :size="14"
                        class="ms-1 text-muted flex-shrink-0"
                    />
                </button>
                <ul class="dropdown-menu p-2 workflow-filter-menu">
                    <li class="mb-1">
                        <div class="input-group input-group-sm">
                            <span class="input-group-text p-1">
                                <LucideIcon
                                    icon="Search"
                                    :size="14"
                                />
                            </span>
                            <input
                                v-model="workflowSearch"
                                type="text"
                                class="form-control form-control-sm"
                                :placeholder="$t('filters.search')"
                                @click.stop=""
                            />
                        </div>
                    </li>
                    <li v-if="!workflowSearch">
                        <a
                            class="dropdown-item small"
                            :class="{ active: filters.workflowId === '' }"
                            @click="selectWorkflow('')"
                        >
                            {{ $t("filters.workflowSelect.none") }}
                        </a>
                    </li>
                    <li v-if="!workflowSearch">
                        <a
                            class="dropdown-item small"
                            :class="{ active: filters.workflowId === 0 }"
                            @click="selectWorkflow(0)"
                        >
                            {{ $t("filters.workflowSelect.withWorkflow") }}
                        </a>
                    </li>
                    <li
                        v-for="workflow in filteredWorkflowsList"
                        :key="workflow.id"
                    >
                        <a
                            class="dropdown-item small"
                            :class="{ active: filters.workflowId === workflow.id }"
                            @click="selectWorkflow(workflow.id)"
                        >
                            {{ workflow.name }}
                        </a>
                    </li>
                </ul>
            </div>
        </div>
        <div class="col-2">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon
                        icon="FileText"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.document"
                    @change="filterData"
                >
                    <option value="1">
                        {{ $t("filters.all") }}
                    </option>
                    <option value="2">
                        {{ $t("filters.singleDocuments") }}
                    </option>
                    <option value="3">
                        {{ $t("filters.batchDocuments") }}
                    </option>
                </select>
            </div>
        </div>
        <div class="col-1">
            <button
                v-tooltip="
                    filters.isAllUsers
                        ? $t('filters.assignment.currentUser')
                        : $t('filters.assignment.allUsers')
                "
                class="btn table-btn btn-sm"
                :class="filters.isAllUsers ? 'btn-outline-secondary' : 'btn-outline-primary'"
                type="button"
                style="display: flex; align-items: center; justify-content: center"
                @click="filterUsers"
            >
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
                    login: this.$store.state.userProfile.login,
                    colType: 2,
                    statusId: "",
                    document: "1",
                },
                workflowSearch: "",
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
                        this.filters.workflows = this.workflowsList.map((t) => t.id);
                        break;
                    default:
                        this.filters.workflows = [this.filters.workflowId];
                }

                this.$emit("filter", { ...this.filters });
            },
            selectWorkflow(id) {
                this.filters.workflowId = id;
                this.workflowSearch = "";
                this.filterData();
            },
            filterUsers() {
                this.filters.isAllUsers = !this.filters.isAllUsers;
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
            filteredWorkflowsList() {
                const search = this.workflowSearch.toLowerCase();
                if (!search) return this.workflowsList;
                return this.workflowsList.filter((w) =>
                    w.name.toLowerCase().includes(search)
                );
            },
            selectedWorkflowLabel() {
                if (this.filters.workflowId === "") {
                    return this.$t("filters.workflowSelect.none");
                }
                if (this.filters.workflowId === 0) {
                    return this.$t("filters.workflowSelect.withWorkflow");
                }
                const found = this.workflowsList.find((w) => w.id === this.filters.workflowId);
                return found ? found.name : this.$t("filters.workflowSelect.none");
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

    .workflow-filter-dropdown .btn {
        font-size: 0.875rem;
        height: calc(1.5em + 0.5rem + 2px);
        padding: 0.25rem 0.5rem;
    }

    .workflow-filter-label {
        font-size: 0.8rem;
        max-width: calc(100% - 20px);
    }

    .workflow-filter-menu {
        min-width: 100%;
        max-height: 260px;
        overflow-y: auto;
    }

    .dropdown-toggle::after {
        display: none;
    }

    .border {
       border-color: var(--color-border-form-control) !important;
    }
</style>
