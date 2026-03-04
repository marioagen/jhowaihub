<template>
    <ModalComponent
        id="questionsHistoryModal"
        :isLoading="isLoading"
        ref="questionsHistoryModalRef"
    >
        <template #header>
            <div class="modal-header">
                <div class="d-flex align-items-center flex-grow-1">
                    <LucideIcon
                        icon="History"
                        :size="20"
                        class="me-2"
                    />
                    <div>
                        <h5 class="modal-title mb-0">
                            {{ $t("analyze.questionsHistoryModal.title") }}
                        </h5>
                        <p class="text-muted small mb-0 mt-1">
                            {{ $t("analyze.questionsHistoryModal.subtitle") }}
                        </p>
                    </div>
                </div>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                    :aria-label="$t('common.close')"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body questions-history-body">
                <!-- Search and filters (fixed, no scroll) -->
                <div class="row g-2 mb-3">
                    <div class="col">
                        <div class="input-group input-group-sm">
                            <span class="input-group-text border-end-0">
                                <LucideIcon
                                    icon="Search"
                                    :size="16"
                                />
                            </span>
                            <input
                                type="text"
                                class="form-control form-control-sm border-start-0"
                                :placeholder="$t('analyze.questionsHistoryModal.searchPlaceholder')"
                                v-model="searchQuery"
                                @keyup.enter="applyFilters"
                            />
                        </div>
                    </div>
                    <div class="col-auto">
                        <div class="input-group input-group-sm">
                            <span class="input-group-text border-end-0">
                                <LucideIcon
                                    icon="User"
                                    :size="16"
                                />
                            </span>
                            <select
                                class="form-select form-select-sm border-start-0"
                                v-model="selectedUser"
                                style="min-width: 140px"
                                @change="onUserFilterChange"
                            >
                                <option value="">
                                    {{ $t("analyze.questionsHistoryModal.allUsers") }}
                                </option>
                                <option
                                    v-for="u in usersList"
                                    :key="u.id"
                                    :value="u.id"
                                >
                                    {{ u.name }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="d-flex align-items-center justify-content-between mb-3">
                    <span class="text-muted small">
                        {{ $t("analyze.questionsHistoryModal.sortedBy") }}: {{ orderLabel }}
                    </span>
                    <button
                        type="button"
                        class="btn btn-outline-secondary btn-sm d-flex align-items-center gap-1"
                        @click="toggleOrder"
                    >
                        <LucideIcon
                            icon="ArrowUpDown"
                            :size="14"
                        />
                        {{ orderLabel }}
                    </button>
                </div>

                <!-- Scrollable area: only the cards list -->
                <div class="conversation-cards-scroll">
                    <div
                        v-if="isLoadingCards"
                        class="conversation-cards-loading"
                    >
                        <LoadingComponent />
                    </div>
                    <template v-else>
                        <div class="conversation-cards">
                            <div
                                v-for="item in conversationCards"
                                :key="item.id"
                                class="conversation-card card border-0 rounded-3 bg-light mb-3"
                            >
                                <div class="card-body p-3">
                                    <div
                                        class="d-flex align-items-center justify-content-between flex-wrap gap-2 mb-2"
                                    >
                                        <div class="d-flex align-items-center text-muted small">
                                            <LucideIcon
                                                icon="User"
                                                :size="14"
                                                class="me-1"
                                            />
                                            <span>
                                                {{ item.userName }}
                                            </span>
                                            <span class="mx-1">·</span>
                                            <LucideIcon
                                                icon="Clock"
                                                :size="14"
                                                class="me-1"
                                            />
                                            <span>
                                                {{ item.date }}
                                            </span>
                                        </div>
                                        <div class="d-flex align-items-center gap-2">
                                            <BadgeComponent
                                                :text="item.tagText"
                                                :variant="item.tagVariant"
                                                :clickable="false"
                                                :class="item.tagClass"
                                            />
                                            <button
                                                type="button"
                                                class="btn btn-link btn-sm p-0 text-secondary"
                                                :title="$t('analyze.copy')"
                                                @click="copyQuestion(item.question)"
                                            >
                                                <LucideIcon
                                                    icon="Copy"
                                                    :size="16"
                                                />
                                            </button>
                                        </div>
                                    </div>
                                    <div class="mb-2">
                                        <div class="d-flex align-items-center gap-1 mb-1">
                                            <LucideIcon
                                                icon="MessageCircle"
                                                :size="14"
                                                class="text-secondary"
                                            />
                                            <span class="fw-bold small text-uppercase">
                                                {{ $t("analyze.question") }}
                                            </span>
                                        </div>
                                        <p class="mb-0 ps-3 small">
                                            {{ item.question }}
                                        </p>
                                    </div>
                                    <div>
                                        <div class="d-flex align-items-center gap-1 mb-1">
                                            <LucideIcon
                                                icon="CheckCircle"
                                                :size="14"
                                                class="text-success"
                                            />
                                            <span class="fw-bold small text-uppercase">
                                                {{ $t("analyze.answer") }}
                                            </span>
                                        </div>
                                        <div
                                            class="ps-3 small"
                                            v-html="item.answerHtml"
                                        ></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div
                            v-if="hasMore"
                            class="text-center py-3"
                        >
                            <button
                                type="button"
                                class="btn btn-outline-primary btn-sm"
                                :disabled="isLoadingCards"
                                @click="loadMore"
                            >
                                {{
                                    isLoadingCards
                                        ? $t("analyze.questionsHistoryModal.loading")
                                        : $t("analyze.questionsHistoryModal.loadMore")
                                }}
                            </button>
                        </div>
                    </template>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer w-100 justify-content-between">
                <span class="text-muted small align-self-center">
                    {{ conversationCards.length }}
                    {{ $t("analyze.questionsHistoryModal.interactionsCount") }}
                </span>
                <button
                    class="btn btn-primary btn-sm"
                    @click="close"
                >
                    {{ $t("common.close") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import UserService from "@/services/users/UserService";

    export default {
        components: {
            ModalComponent,
            BadgeComponent,
            LoadingComponent,
        },
        emits: ["update"],
        data: () => ({
            isLoading: false,
            isLoadingCards: false,
            documentId: null,
            searchQuery: "",
            selectedUser: "",
            usersList: [],
            conversationCards: [],
            currentTake: 10,
            hasMore: false,
            filters: {
                search: "",
                user: "",
                order: "desc",
                orderBy: "created",
            },
        }),
        computed: {
            orderLabel() {
                return this.filters.order === "asc"
                    ? this.$t("analyze.questionsHistoryModal.orderOldest")
                    : this.$t("analyze.questionsHistoryModal.orderNewest");
            },
        },
        methods: {
            open(documentId) {
                this.documentId = documentId;
                this.searchQuery = "";
                this.selectedUser = "";
                this.conversationCards = [];
                this.currentTake = 10;
                this.hasMore = false;
                this.$refs.questionsHistoryModalRef?.open();
                this.loadUsers();
                this.getHistory();
            },
            loadUsers() {
                UserService.getAllUsers()
                    .then((data) => {
                        if (data?.error) {
                            this.usersList = [];
                            return;
                        }
                        this.usersList = Array.isArray(data) ? data : [];
                    })
                    .catch(() => {
                        this.usersList = [];
                    });
            },
            onUserFilterChange() {
                this.filters.user = this.selectedUser;
                this.currentTake = 10;
                this.getHistory();
            },
            close() {
                this.$refs.questionsHistoryModalRef?.close();
            },
            copyQuestion(text) {
                navigator.clipboard.writeText(text);
            },
            formatHistoryDate(created) {
                if (!created) return "—";
                const d = new Date(created);
                const pad = (n) => String(n).padStart(2, "0");
                return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
            },
            getHistory() {
                this.isLoadingCards = true;
                this.filters.user = this.selectedUser;
                const filters = {
                    take: this.currentTake,
                    search: this.searchQuery,
                    user: this.selectedUser || undefined,
                    order: this.filters.order,
                    orderBy: this.filters.orderBy,
                };
                DocumentsServices.getDocumentQuestionsHistory(this.documentId, filters)
                    .then((response) => {
                        if (response?.error) {
                            this.conversationCards = [];
                            this.hasMore = false;
                            return;
                        }
                        const list = Array.isArray(response) ? response : [];
                        this.conversationCards = list.map((entry) => {
                            const isQuestionario = entry.type === 1;
                            return {
                                id: entry.id,
                                userName: entry.userName || "—",
                                date: this.formatHistoryDate(entry.created),
                                tagText: isQuestionario
                                    ? this.$t("analyze.questionsHistoryModal.tagQuestionnaire")
                                    : this.$t("analyze.questionsHistoryModal.tagFreeQuestion"),
                                tagVariant: "primary",
                                tagClass: isQuestionario
                                    ? "badge-tag-questionario"
                                    : "badge-tag-pergunta-livre",
                                question: entry.input || "",
                                answerHtml: entry.output || "",
                            };
                        });
                        this.hasMore = list.length >= filters.take;
                    })
                    .catch(() => {
                        this.conversationCards = [];
                        this.hasMore = false;
                    })
                    .finally(() => {
                        this.isLoadingCards = false;
                    });
            },
            loadMore() {
                this.currentTake += 10;
                this.getHistory();
            },
            applyFilters() {
                this.currentTake = 10;
                this.getHistory();
            },
            toggleOrder() {
                this.filters.order = this.filters.order === "desc" ? "asc" : "desc";
                this.currentTake = 10;
                this.getHistory();
            },
        },
    };
</script>
<style scoped>
    .questions-history-body {
        display: flex;
        flex-direction: column;
        min-height: 0;
    }

    .conversation-cards-scroll {
        flex: 1 1 auto;
        min-height: 0;
        overflow-y: auto;
        max-height: 65vh;
    }

    .conversation-cards-loading {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 200px;
        padding: 2rem;
    }

    .conversation-card {
        background-color: var(--bs-light, #f8f9fa) !important;
    }

    .badge-tag-questionario {
        background-color: #e8e0f0 !important;
        color: #6b5b7a !important;
    }

    .badge-tag-pergunta-livre {
        background-color: #cce5ff !important;
        color: #004085 !important;
    }

    .modal-header .modal-title {
        font-weight: 600;
    }
</style>
<style>
    #questionsHistoryModal.modal .modal-dialog {
        max-width: 800px;
        min-height: 85vh;
    }

    #questionsHistoryModal.modal .modal-content {
        min-height: 85vh;
    }
</style>
