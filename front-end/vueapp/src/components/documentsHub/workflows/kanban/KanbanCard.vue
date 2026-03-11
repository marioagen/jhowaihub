<template>
    <div
        class="card"
        :class="{ 'batch-card': dataCard.isBatchParent }"
    >
        <div class="card-content">
            <div class="card-body pb-0">
                <div class="mb-1 title-text">
                    <LucideIcon
                        v-if="dataCard.isBatchParent"
                        icon="Files"
                        :size="16"
                        class="batch-icon me-1"
                    />
                    {{ truncateText(dataCard.name) }}
                    <div
                        v-if="isCardRejected"
                        class="badge fr flex-shrink-1 mt-1"
                        :style="badgeStyle(dataCard.status.color)"
                    >
                        {{ $t("workflow.statusList." + dataCard.status.name.toLowerCase()) }}
                    </div>
                    <div
                        v-else
                        class="badge fr flex-shrink-1 mt-1"
                        :style="badgeStyle(dataStep.status.color)"
                    >
                        {{ $t("workflow.statusList." + dataStep.status.name.toLowerCase()) }}
                    </div>
                </div>
                <div class="mb-2">
                    <LucideIcon
                        icon="FileText"
                        :size="12"
                        class="me-1"
                    />
                    <small>
                        {{ dataCard.description }}
                    </small>
                </div>
                <div class="mb-2">
                    <LucideIcon
                        icon="Calendar"
                        :size="12"
                        class="me-1"
                    />
                    <small>
                        {{ formatDate(dataCard.created) }}
                    </small>
                </div>
                <hr />
                <div class="mb-2 overflow-x">
                    <LucideIcon
                        icon="User"
                        size="12"
                        class="me-1"
                    />
                    <small class="user">
                        {{ $t("card.userApplicant") }}:
                        {{ dataCard.owner }}
                    </small>
                </div>
                <div
                    v-if="!isLastStep && dataCard.assignedUser && !showLoading"
                    class="mb-2"
                >
                    <LucideIcon
                        icon="User"
                        size="12"
                        class="me-1"
                    />
                    <small class="user">
                        {{ $t("card.userApplicant") }}:
                        {{ dataCard.assignedUser.name }}
                    </small>
                    <button
                        type="button"
                        @click.stop="unassignUser"
                        class="btn btn-sm btn-unlink ms-1 px-1"
                        v-tooltip.right="$t('card.unassignInfo')"
                    >
                        <LucideIcon
                            v-if="isUnassigningUser"
                            icon="Loader"
                            :size="16"
                            class="mr-2 animate-spin text-white"
                        />
                        <LucideIcon
                            v-else
                            icon="Unlink"
                            size="16"
                            class="unlink-icon"
                        />
                    </button>
                </div>
            </div>
            <div
                class="card-footer pt-0"
                :class="showLoading ? 'padding-loading ' : ''"
            >
                <div
                    class="mb-2 d-flex justify-content-between align-items-center flex-wrap"
                    v-if="!showLoading"
                >
                    <div class="d-flex align-items-center gap-2">
                        <button
                            type="button"
                            class="btn btn-sm btn-primary"
                            @click="redirectToAnalyzer"
                        >
                            {{ $t("common.analyze") }}
                            <LucideIcon
                                icon="FileClock"
                                :size="15"
                                class="ms-2"
                            />
                        </button>
                        <button
                            v-if="isLastStep"
                            type="button"
                            class="btn btn-sm btn-outline-success"
                            @click.stop="openFinalizeConfirm"
                        >
                            <LucideIcon
                                icon="Check"
                                :size="15"
                            />
                            {{ $t("common.finalize") }}
                        </button>
                    </div>
                    <div v-if="!isLastStep">
                        <button
                            v-if="!isFirstStep || dataCard.assignedUser"
                            class="btn btn-sm btn-outline-primary float-end"
                            @click.stop="advanceStep"
                        >
                            <span>
                                {{ $t("common.advance") }}
                            </span>
                            <LucideIcon
                                icon="ChevronRight"
                                :size="16"
                                class="me-1"
                                v-if="!isLoadingAnalysis"
                            />
                            <div
                                class="spinner-grow text-light"
                                role="status"
                                v-if="isLoadingAnalysis"
                            ></div>
                        </button>
                        <div v-else-if="!dataCard.assignedUser && !showLoading">
                            <div
                                v-if="isAdmin"
                                class="btn-group"
                            >
                                <button
                                    type="button"
                                    class="btn btn-sm btn-primary btn-outline-primary dropdown-toggle"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false"
                                    @click.stop=""
                                >
                                    <LucideIcon
                                        v-if="isUpdatingAssignedUser"
                                        icon="Loader"
                                        :size="16"
                                        class="mr-2 animate-spin text-white"
                                    />
                                    <span>
                                        {{ $t("card.assignBtn") }}
                                    </span>
                                    <LucideIcon
                                        icon="ChevronRight"
                                        :size="15"
                                        class="ml-2 icon-closed"
                                    />
                                    <LucideIcon
                                        icon="ChevronDown"
                                        :size="15"
                                        class="ml-2 icon-open"
                                    />
                                </button>
                                <ul class="dropdown-menu p-2 users-list">
                                    <li
                                        v-if="users.length > 5"
                                        class="mb-1"
                                    >
                                        <div class="input-group input-group-sm">
                                            <span class="input-group-text p-1">
                                                <LucideIcon
                                                    icon="Search"
                                                    :size="16"
                                                    class="me-1"
                                                />
                                            </span>
                                            <input
                                                :id="`filter-user-${dataCard.id}`"
                                                v-model="userSearchText"
                                                type="text"
                                                name="filter"
                                                class="form-control"
                                                @input="searchUser"
                                                @click.stop=""
                                            />
                                        </div>
                                    </li>
                                    <li
                                        v-for="user in filteredUsers"
                                        :key="user.id"
                                        @click.stop="assignUser(user.id)"
                                    >
                                        <span class="dropdown-item">
                                            {{ user.name }}
                                        </span>
                                    </li>
                                </ul>
                            </div>
                            <button
                                v-else
                                type="button"
                                class="btn btn-sm btn-primary assing-btn"
                                @click.stop="assignUser(loggedUserId)"
                            >
                                <LucideIcon
                                    v-if="isUpdatingAssignedUser"
                                    icon="Loader"
                                    :size="12"
                                    class="mr-2 animate-spin text-white"
                                />
                                {{ $t("card.assignBtn") }}
                                <LucideIcon
                                    icon="NotebookPen"
                                    size="16"
                                    class="ml-2"
                                />
                            </button>
                        </div>
                    </div>
                </div>
                <div
                    class="cover"
                    v-if="showLoading"
                >
                    <div class="spinner-cover">
                        <LucideIcon
                            icon="Loader"
                            :size="24"
                            class="me-1 animate-spin"
                        />
                    </div>
                    <div
                        v-if="showLoading"
                        class="progress-content"
                    >
                        <div class="mb-2">
                            {{ $t("common.processing") }}
                            {{ truncatedToolName }}
                            <span class="float-end">{{ dataCard.percentage || 0 }}%</span>
                        </div>
                        <div class="progress">
                            <div
                                class="progress-bar progress-bar-striped progress-bar-animated"
                                role="progressbar"
                                :aria-valuenow="dataCard.percentage || 0"
                                aria-valuemin="0"
                                aria-valuemax="100"
                                :style="{
                                    width: (dataCard.percentage || 0) + '%',
                                }"
                            ></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <ConfirmModal
            :id="`finalize-modal-${dataCard.id}`"
            ref="finalizeConfirmModalRef"
            :isLoading="isFinalizing"
            :title="'card.finalizeConfirmTitle'"
            :message="'card.finalizeConfirmMessage'"
            :confirmText="'common.finalize'"
            cancelText="common.cancel"
            confirmVariant="success"
            iconVariant="success"
            iconeName="Check"
            @confirm="onConfirmFinalize"
        />
    </div>
</template>
<script>
    import CardsServices from "@/services/cards/CardsServices";
    import StatusService from "@/services/status/StatusService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import dates from "@/helpers/date";

    export default {
        name: "CardComponent",
        components: { ConfirmModal },
        emits: ["reload", "cardMoved", "cardUpdated", "cardFinalized"],
        data: () => ({
            isLoadingAnalysis: false,
            isUpdatingAssignedUser: false,
            isUnassigningUser: false,
            isFinalizing: false,
            statusProgress: null,
            signalrEventStatusChanged: "StatusChanged",
            userSearchText: "",
            filteredUsers: [],
            finalizeStatusId: null,
        }),
        props: {
            dataCard: {
                type: Object,
                required: true,
                default: () => {},
            },
            dataStep: {
                type: Object,
                required: true,
                default: () => {},
            },
            isFirstStep: {
                type: Boolean,
                required: true,
                default: false,
            },
            isLastStep: {
                type: Boolean,
                required: true,
                default: false,
            },
            users: {
                type: [Array, Object],
                required: true,
                default: () => {},
            },
        },
        methods: {
            badgeStyle(color) {
                return {
                    "--cor-base": color,
                    color: "var(--cor-base)",
                    backgroundColor: "color-mix(in srgb, var(--cor-base) 30%, white)",
                };
            },
            async updateStatus(nextStepOrder = null) {
                const targetOrder = nextStepOrder ?? this.dataStep.order + 1;
                if (this.isLastStep && nextStepOrder === null) {
                    return;
                }
                var params = {
                    CardId: this.dataCard.id,
                    NextStepOrder: targetOrder,
                    WorkflowId: this.dataStep.workflowId,
                };
                const response = await CardsServices.updateStepAndStatus(params);
                if (response?.error !== undefined) {
                    throw new Error(response.error.response?.data?.labelError);
                }
            },
            async assignUser(userId) {
                var params = {
                    CardId: this.dataCard.id,
                    UserId: userId,
                };
                this.isUpdatingAssignedUser = true;
                const response = await CardsServices.assignUser(params);
                if (response?.error !== undefined) {
                    this.$notify({
                        title: "Error",
                        message: response.error,
                        variant: "danger",
                        icon: "CircleX",
                    });
                } else {
                    const assignedUser = Array.isArray(this.users)
                        ? this.users.find((u) => u.id === userId)
                        : null;

                    this.$emit("cardUpdated", {
                        card: {
                            ...this.dataCard,
                            assignedUser: assignedUser || null,
                        },
                        currentStepOrder: this.dataStep.order,
                        newStepOrder: this.dataStep.order,
                    });
                }
                this.isUpdatingAssignedUser = false;
            },
            async unassignUser() {
                this.isUnassigningUser = true;
                const response = await CardsServices.unassignUser(this.dataCard.id);
                if (response?.error !== undefined) {
                    this.$notify({
                        title: "Error",
                        message: response.error,
                        variant: "danger",
                        icon: "CircleX",
                    });
                } else {
                    this.$emit("cardUpdated", {
                        card: {
                            ...this.dataCard,
                            assignedUser: null,
                        },
                        currentStepOrder: this.dataStep.order,
                        newStepOrder: this.dataStep.order,
                    });
                }
                this.isUnassigningUser = false;
            },
            async advanceStep() {
                this.isLoadingAnalysis = true;
                try {
                    await this.updateStatus();
                    this.reloadList();
                } catch (e) {
                    this.$notify({
                        title: "Error",
                        message: this.$t("card.errorAdvancingCard"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoadingAnalysis = false;
                }
            },
            openFinalizeConfirm() {
                this.$refs.finalizeConfirmModalRef?.open();
            },
            async onConfirmFinalize() {
                this.isFinalizing = true;
                try {
                    await this.updateStatusOnly();
                    this.$refs.finalizeConfirmModalRef?.close();
                    this.$emit("cardFinalized", {
                        cardId: this.dataCard.id,
                        currentStepOrder: this.dataStep.order,
                    });
                    this.reloadList();
                    this.$notify({
                        title: this.$t("common.success"),
                        message: this.$t("card.successFinalizeMessage"),
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (e) {
                    this.$notify({
                        title: this.$t("common.error"),
                        message: e.message || this.$t("card.errorFinalizingCard"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isFinalizing = false;
                }
            },
            async updateStatusOnly() {
                const params = {
                    CardId: this.dataCard.id,
                    StatusId: this.finalizeStatusId,
                };
                const response = await CardsServices.updateStatusOnly(params);
                if (response?.error !== undefined) {
                    throw new Error(response.error.response?.data?.labelError);
                }
            },
            redirectToAnalyzer() {
                if (!this.showLoading) {
                    this.$router.push({
                        name: "Analyzer",
                        params: {
                            documentId: this.dataCard.documentId,
                            cardId: this.dataCard.id,
                        },
                        query: { page: this.backPage },
                    });
                }
            },
            reloadList() {
                this.$emit("reload");
            },
            searchUser() {
                const searchText = this.userSearchText.toLowerCase();
                this.filteredUsers = this.users.filter(
                    (o) => o.name && o.name.toLowerCase().includes(searchText)
                );
            },
            setUsers() {
                this.filteredUsers = this.users;
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            truncateText(text) {
                if (!text) return "";
                return text.length > 25 ? text.substring(0, 25) + "..." : text;
            },
        },
        async mounted() {
            this.setUsers();
            const statusResponse = await StatusService.getStatus();
            if (statusResponse?.error === undefined && Array.isArray(statusResponse)) {
                const finalize = statusResponse.find(
                    (s) => s.name && s.name.toLowerCase() === "finalize"
                );
                if (finalize) this.finalizeStatusId = finalize.id;
            }
        },
        computed: {
            showLoading() {
                return this.dataCard.percentage < 100;
            },
            isAdmin() {
                return this.$store.state.userProfile.isAdmin;
            },
            loggedUserId() {
                const user = this.users.find(
                    (u) => u.email === this.$store.state.userProfile.login
                );
                return user ? user.id : null;
            },
            truncatedToolName() {
                if (!this.dataCard?.toolName) return "";
                const toolName = this.dataCard.toolName.trim();
                return toolName.length > 10 ? toolName.substring(0, 10) + "..." : toolName;
            },
            isCardRejected() {
                return this.dataCard.status.name.toLowerCase() === "rejected";
            },
        },
    };
</script>
<style scoped>
    .card {
        --kanban-card-title-size: 0.6rem;
        --kanban-card-text-size: 0.6rem;
        --kanban-card-badge-size: 0.5rem;
        --kanban-card-button-size: 0.6rem;
        --kanban-card-margin: 0;
        --kanban-card-padding: 0.5rem;
        margin-top: var(--kanban-card-margin) !important;
        margin-bottom: var(--kanban-card-margin) !important;
        white-space: nowrap;
    }

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-warning {
        background-color: #fef9c2 !important;
        color: #a65f00 !important;
    }

    .bg-danger {
        background-color: #ffedd4 !important;
        color: #ca3500 !important;
    }

    .bg-success {
        background-color: #d0fae5 !important;
        color: #007a55 !important;
    }

    .card-content {
        position: relative;
    }

    .progress-content {
        width: 100%;
        z-index: 11;
        position: relative;
        padding: var(--kanban-card-padding) !important;
        font-size: var(--kanban-card-text-size) !important;
    }

    .progress-content .progress {
        height: 10px;
    }

    .spinner-cover {
        position: absolute;
        inset: calc(0.25rem * 0);
        align-items: center;
        display: flex;
        justify-content: center;
        z-index: 10;
        background-color: var(--color-card-content);
        opacity: 0.8;
    }

    .hide-card div,
    .hide-card p {
        color: transparent;
        height: 15px;
        background: linear-gradient(
            90deg,
            var(--skeleton-base) 25%,
            var(--skeleton-highlight) 37%,
            var(--skeleton-base) 63%
        );
        background-size: 400% 100%;
        animation: shimmer 1.4s ease infinite;
        border-radius: 8px;
    }

    @keyframes shimmer {
        0% {
            background-position: -400px 0;
        }

        100% {
            background-position: 400px 0;
        }
    }

    .hide-card .footer {
        display: none;
    }

    .animate-spin {
        animation: spin 1s linear infinite;
        color: var(--color-bg-icon-active);
    }

    @keyframes spin {
        100% {
            transform: rotate(360deg);
        }
    }

    .card .card-body p {
        font-size: var(--kanban-card-title-size) !important;
        font-weight: bold !important;
        overflow-wrap: break-word;
        white-space: normal;
    }

    .card-body small {
        font-size: var(--kanban-card-text-size);
        overflow-wrap: break-word;
        white-space: normal;
    }

    .card-body small.user {
        overflow-wrap: break-word;
        white-space: normal;
    }

    .card-body .badge {
        font-size: var(--kanban-card-badge-size);
        max-width: 60%;
        overflow-wrap: break-word;
        white-space: normal;
    }

    /* Add padding to card body for space from borders, but keep internal spacing removed */
    .card-body {
        padding: var(--kanban-card-padding) !important;
    }

    /* Remove margins from all elements inside card-body */
    .card-body > * {
        margin: 0 !important;
    }

    .card-body .mb-2 {
        margin-bottom: 0 !important;
    }

    .card-body p {
        margin-bottom: 0 !important;
    }

    .card-body hr {
        margin: 0 !important;
    }

    .card-footer {
        background-color: inherit;
        border-top-width: 0px;
        padding: var(--kanban-card-padding) !important;
    }

    /* Remove margins from footer elements, but keep spacing for progress-content */
    .card-footer > *:not(.cover) {
        margin: 0 !important;
    }

    .card-footer .mb-2:not(.progress-content .mb-2),
    .card-footer .mb-1:not(.progress-content .mb-1) {
        margin-bottom: 0 !important;
    }

    /* Reduce spacing around badge and button */
    .card-footer .d-flex {
        margin: 0 !important;
        gap: 0.25rem;
    }

    .card-footer .badge {
        margin: 0 !important;
    }

    .card-footer .btn {
        margin: 0 !important;
    }

    /* Ensure progress-content keeps its spacing */
    .progress-content .mb-2 {
        margin-bottom: 0.5rem !important;
    }

    .card-footer .btn {
        font-size: var(--kanban-card-button-size);
    }

    .overlay-loading {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(255, 255, 255, 0.7);
        z-index: 9999;
        display: flex;
        align-items: center;
        justify-content: center;
        width: 3rem;
        height: 3rem;
    }

    .fr {
        float: right;
    }

    .spinner-grow {
        width: 1rem;
        height: 1rem;
        margin-left: 5px;
    }

    hr {
        margin: 0 !important;
    }

    .dropdown-toggle::after {
        display: none;
    }

    .dropdown-toggle .icon-closed {
        display: inline-block;
    }

    .dropdown-toggle .icon-open {
        display: none;
    }

    .dropdown-toggle.show .icon-closed {
        display: none;
    }

    .dropdown-toggle.show .icon-open {
        display: inline-block;
    }

    .btn-unlink {
        background-color: orange;
        line-height: 1.3;
    }

    .unlink-icon {
        vertical-align: sub;
        color: white;
    }

    .users-list {
        max-height: 300px;
        overflow-y: auto;
    }

    .users-list .dropdown-item {
        font-size: var(--kanban-card-text-size);
    }

    .padding-loading {
        padding-bottom: 50px;
    }

    .title-text {
        font-size: 13px;
    }

    .batch-card {
        border-top: 3px solid #2b7fff !important;
        border: 1px solid #2b7fff;
    }

    .batch-icon {
        color: #2b7fff;
        vertical-align: middle;
    }
</style>
