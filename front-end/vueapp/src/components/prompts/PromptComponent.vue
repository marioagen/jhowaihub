<template>
    <div class="mb-2 d-flex align-items-center gap-2 scroll-area">
        <div class="form-pill">
            <a
                class="pill-link"
                href="#"
                :class="{ selected: !loadAllPrompts }"
                @click.prevent="!loadAllPrompts ? getAllPrompts() : null"
            >
                <span
                    class="badge rounded-pill"
                    :class="{ border: loadAllPrompts }"
                >
                    {{ $t("common.all") }}
                </span>
            </a>
        </div>
        <div class="form-pill">
            <a
                class="pill-link"
                href="#"
                :class="{ selected: loadAllPrompts }"
                @click.prevent="loadAllPrompts ? getOnlyUserPrompts() : null"
            >
                <span
                    class="badge rounded-pill"
                    :class="{ border: !loadAllPrompts }"
                >
                    {{ $t("prompts.myPromptsBadge") }}
                </span>
            </a>
        </div>
        <div
            class="form-check"
            v-if="this.dataPrompt.length > 1 && !loadAllPrompts"
        >
            <input
                class="form-check-input"
                type="checkbox"
                value=""
                @click="checkAll($event)"
            />
            <label>{{ $t("common.selectAll") }} &nbsp;</label>
        </div>
        <button
            type="button"
            class="btn delete-custom d-flex align-items-center"
            @click="confirmationDialog()"
            v-if="this.listIds.length > 0"
        >
            <i class="fas fa-trash text-danger icon-delete"></i>
            {{ $t("common.delete") }}
        </button>
    </div>
    <div
        class="row loading-container"
        v-if="dataPrompt.length === 0 && !loading"
    >
        <div class="data-load">
            <i class="fas fa-exclamation-circle text-secondary"></i>
            &nbsp;{{ $t("prompts.noPromptsListWereFound") }}.
        </div>
    </div>
    <div
        class="row loading-container"
        v-if="loading"
    >
        <div class="data-load">
            <i class="fas fa-sync-alt fa-spin text-secondary"></i>
            &nbsp;{{ $t("common.loading") }}..
        </div>
    </div>
    <div></div>
    <div
        class="row card-list scroll-area pb-3"
        v-if="!loading"
    >
        <div
            v-for="item in filteredPrompts"
            :key="item.id"
            class="card"
        >
            <div class="card-body mt-2">
                <div class="row">
                    <div class="col-12 icons-card">
                        <LucideIcon
                            v-if="item.isImported && !item.isEdited"
                            icon="Globe"
                            :size="16"
                            class="text-primary mt-2"
                        />
                        <span
                            v-else
                            class="dot mt-2 m-1"
                        ></span>
                        <span class="m-1">
                            {{ item.name }}
                        </span>
                        <div class="custom-margin">
                            <input
                                class="form-check-input checkbox m-2"
                                type="checkbox"
                                value=""
                                :id="item.id"
                                @click="countChecks(item.id)"
                                v-if="item.isOwner"
                            />
                            <a
                                href="#"
                                class="m-1"
                                id="dropdownIcon"
                                data-bs-toggle="dropdown"
                                aria-expanded="false"
                            >
                                <i class="fas fa-ellipsis-v icon-ellipsis"></i>
                            </a>
                            <ul
                                class="dropdown-menu"
                                aria-labelledby="dropdownIcon"
                            >
                                <li
                                    @click="redirectToEditPrompt(item.id)"
                                    v-if="item.isOwner"
                                >
                                    <a class="dropdown-item">
                                        {{ $t("common.edit") }}
                                    </a>
                                </li>
                                <li @click="redirectToClonePrompt(item.id)">
                                    <a class="dropdown-item">
                                        {{ $t("prompts.cloneAction") }}
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
                <p class="card-text d-flex align-items-start gap-1">
                    <span>
                        {{
                            item.description && item.description.length > 100
                                ? item.description.slice(0, 100) + "..."
                                : item.description
                        }}
                    </span>
                    <span
                        v-if="item.description && item.description.length > 100"
                        class="description-view-icon"
                        v-tooltip="item.description"
                        tabindex="0"
                    >
                        <LucideIcon
                            icon="Eye"
                            :size="14"
                        />
                    </span>
                </p>
            </div>
            <div class="card-footer">
                <div class="date-info">
                    <i class="far fa-clock mt-1"></i>
                    <span>
                        &ensp;{{ $t("dashboard.created") }}
                        {{ this.formatDate(item.created) }}
                    </span>
                </div>
                <div class="owner-info d-flex align-items-center">
                    <span class="owner-label">{{ $t("common.owner") }}:</span>
                    <span
                        v-if="item.ownerName || item.ownerEmail"
                        class="owner-avatar-wrapper"
                        v-tooltip="ownerTooltip(item)"
                    >
                        <AvatarComponent
                            :name="item.ownerName || item.ownerEmail || ''"
                            variant="primary"
                            :size="28"
                        />
                    </span>
                    <span
                        v-else
                        class="owner-avatar-placeholder"
                        v-tooltip="'-'"
                    >
                        —
                    </span>
                </div>
            </div>
        </div>
    </div>
    <div
        class="row mt-1"
        v-if="!loading && this.dataPrompt.length < this.pagination.count"
    >
        <div class="col">
            <div class="pagination justify-content-center">
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="loadMore"
                >
                    {{ $t("prompts.labelLoadMore") }}
                </button>
            </div>
        </div>
    </div>
    <ConfirmModal
        id="deletePromptsConfirm"
        title="prompts.removeAllPrompts"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deletePrompts"
    />
</template>
<script>
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import AvatarComponent from "@/components/global/AvatarComponent.vue";
    import PromptService from "@/services/prompts/PromptsService";
    export default {
        name: "PromptComponent",
        emits: ["showAlertToast"],
        data() {
            return {
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "Type",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                searchInput: "",
                searching: false,
                dataPrompt: [],
                loading: false,
                pagination: {
                    currentPage: 0,
                    count: 0,
                    totalPages: 0,
                },
                isDeleting: false,
                isAscending: false,
                dataModal: {},
                colType: 2,
                selectedOption: 9,
                listIds: [],
                loadAllPrompts: true,
                filters: {
                    input: "",
                    name: "",
                    desc: "",
                    created: "",
                },
            };
        },
        components: {
            ConfirmModal,
            AvatarComponent,
        },
        methods: {
            ownerTooltip(item) {
                if (!item.ownerName && !item.ownerEmail) return "-";
                const parts = [];
                if (item.ownerName) parts.push(item.ownerName);
                if (item.ownerEmail) parts.push(item.ownerEmail);
                return parts.join("\n");
            },
            checkAll(event) {
                const checkboxes = document.querySelectorAll(".checkbox");
                let checkboxIds = [];
                this.listIds = [];
                checkboxes.forEach((checkbox) => {
                    checkbox.checked = event.target.checked;
                    checkboxIds.push(parseInt(checkbox.id));
                });
                this.countMultipleChecks(checkboxIds);
            },
            countChecks(id) {
                let checkBox = document.querySelector(`input[type="checkbox"][id="${id}"]`);
                if (checkBox && checkBox.checked) {
                    this.listIds.push(id);
                } else {
                    this.listIds = this.listIds.filter((i) => i !== id);
                }
            },
            countMultipleChecks(checkboxIds) {
                parseInt(checkboxIds);
                checkboxIds.forEach((id) => {
                    let checkBox = document.querySelector(`input[type="checkbox"][id="${id}"]`);
                    if (checkBox && checkBox.checked) {
                        this.listIds.push(id);
                    } else {
                        this.listIds = this.listIds.filter((i) => i !== id);
                    }
                });
            },
            redirectToNewPrompt(prompt) {
                this.$router.push({
                    name: "PromptNew",
                    query: { name: prompt },
                });
            },
            redirectToClonePrompt(id) {
                this.$router.push({
                    name: "PromptNew",
                    query: { clone: id },
                });
            },
            redirectToEditPrompt(id) {
                this.$router.push({
                    name: "PromptNew",
                    query: { id: id },
                });
            },
            getList(obj) {
                this.dataPrompt = [];
                this.listIds = [];
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                var paramsReq = {
                    search: this.filters.input,
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };
                PromptService.getPromptList(paramsReq).then((response) => {
                    if (response.error !== undefined) {
                        return this.$notify({
                            title: "prompt.title",
                            message: response.error,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.dataPrompt = response.data.items;
                    this.pagination = {
                        currentPage: response.data.currentPage,
                        count: response.data.count,
                        totalPages: response.data.totalPages,
                    };
                    this.loading = false;
                });
            },
            confirmationDialog() {
                this.$refs.DeleteDialog?.open();
            },
            deletePrompts() {
                this.isDeleting = true;
                PromptService.deletePrompts(this.listIds)
                    .then((response) => {
                        if (!response) {
                            return this.$notify({
                                title: "prompts.title",
                                message: "prompts.deleteError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                        this.$refs.DeleteDialog?.close();
                        this.$notify({
                            title: "prompts.title",
                            message: "prompts.deleteSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                        this.getList({
                            search: "",
                            page: this.queryPage,
                            type: null,
                        });
                    })
                    .catch(() => {
                        this.$notify({
                            title: "prompts.title",
                            message: "prompts.deleteError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isDeleting = false;
                    });
            },
            formatDate(dataObj) {
                const date = new Date(dataObj);
                let formattedDate =
                    `${String(date.getDate()).padStart(2, "0")}/` +
                    `${String(date.getMonth() + 1).padStart(2, "0")}/` +
                    `${date.getFullYear()}`;
                return formattedDate;
            },
            loadMore() {
                this.selectedOption = this.selectedOption * 2;
                this.getList({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            getAllPrompts() {
                this.loadAllPrompts = true;
                this.getList({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            getOnlyUserPrompts() {
                this.loadAllPrompts = false;
                this.getUserPrompts({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            getUserPrompts(obj) {
                var userId;
                this.dataPrompt = [];
                this.listIds = [];
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };
                PromptService.getPromptByUserId(paramsReq, userId).then((response) => {
                    if (response.error !== undefined) {
                        return this.$notify({
                            title: "prompt.title",
                            message: response.error,
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.dataPrompt = response.data.items;
                    this.pagination = {
                        currentPage: response.data.currentPage,
                        count: response.data.count,
                        totalPages: response.data.totalPages,
                    };
                    this.loading = false;
                });
            },
        },
        created() {
            this.getList({
                search: "",
                page: this.queryPage,
                type: null,
            });
        },
        computed: {
            filteredPrompts() {
                const search = (this.filters.input || "").toLowerCase();
                return this.dataPrompt.filter((item) => {
                    const nameMatch = item.name && item.name.toLowerCase().includes(search);
                    const descMatch =
                        item.description && item.description.toLowerCase().includes(search);
                    const createdStr = item.created
                        ? new Date(item.created).toLocaleDateString("pt-BR")
                        : "";
                    const createdMatch =
                        createdStr.includes(search) ||
                        (item.created && item.created.toLowerCase().includes(search));
                    if (!search) return true;
                    return nameMatch || descMatch || createdMatch;
                });
            },
        },
        mounted() {},
        unmounted() {},
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
        max-height: 400px;
        overflow-y: auto;
        min-height: 20%;
    }

    .card-list {
        display: flex;
        flex-wrap: wrap;
        gap: 1rem;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }

    .card {
        flex: 0 1 calc(33.333% - 1rem);
        height: auto;
        background-color: var(--color-card-content) !important;
        color: var(--color-body-content) !important;
        border-color: var(--color-border-form-control) !important;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 38%);
    }

    .card-body {
        padding: 0;
    }

    .icons-card {
        display: flex;
    }

    #dropdownIcon {
        color: var(--color-body-content) !important;
    }

    .date-info {
        display: flex;
        align-items: center;
        color: #0073ea !important;
    }

    .card-footer {
        background-color: initial;
        border-top: none;
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        justify-content: space-between;
        gap: 0.5rem;
        padding-left: 0;
        padding-right: 0;
    }

    .owner-info {
        flex-shrink: 0;
    }

    .owner-label {
        font-size: 0.85rem;
        color: var(--color-body-content);
        margin-right: 0.35rem;
    }

    .owner-avatar-wrapper {
        display: inline-flex;
        cursor: default;
    }

    .owner-avatar-wrapper :deep(.btn-primary) {
        background-color: #0073ea !important;
        border-color: #0073ea !important;
        color: #ffffff !important;
    }

    .owner-avatar-wrapper :deep(.me-3) {
        margin-right: 0.25rem !important;
    }

    .owner-avatar-placeholder {
        font-size: 0.9rem;
        color: var(--color-body-content);
        opacity: 0.7;
    }

    .card-list {
        padding: 0px 10px;
    }

    @media (max-width: 768px) {
        .card-list {
            display: list-item;
        }
    }

    .data-load {
        background-color: var(--color-bg-loading-content) !important;
        border-color: var(--color-bg-loading-content) !important;
        color: var(--color-body-content) !important;
        text-align: center;
        padding: 9px;
        border-bottom-width: 2px;
        border-radius: 10px;
    }

    .loading-container {
        padding-left: 10px;
        padding-right: 10px;
    }

    .icon-delete {
        font-size: 0.9em;
        margin-right: 8px;
    }

    .icon-ellipsis {
        margin-top: 8px;
    }

    .custom-margin {
        margin-left: auto;
    }

    .dot {
        height: 10px;
        width: 10px;
        background-color: #ff6900;
        border-radius: 50%;
        display: inline-block;
    }

    .description-view-icon {
        flex-shrink: 0;
        cursor: pointer;
        color: var(--color-body-content);
    }

    .description-view-icon:hover {
        opacity: 0.8;
    }

    .badge {
        background-color: var(--color-bg-badge) !important;
        color: var(--color-body-content) !important;
    }

    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
</style>
