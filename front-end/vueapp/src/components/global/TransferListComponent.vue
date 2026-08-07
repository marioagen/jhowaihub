<template>
    <div class="container mt-1">
        <div class="row">
            <div class="col">
                <div class="transfer-list-header">
                    <h6 class="mb-0">{{ $t(transferListTitle) }}</h6>
                    <div
                        v-if="showSortControl"
                        class="transfer-list-sort"
                    >
                        <span class="transfer-list-sort-label">{{ $t("filters.sortBy") }}</span>
                        <div
                            class="btn-group btn-group-sm"
                            role="group"
                            :aria-label="$t('filters.sortBy')"
                        >
                            <button
                                type="button"
                                class="btn transfer-list-sort-button"
                                :class="{ active: availableSort === 'alphabetical' }"
                                :aria-pressed="availableSort === 'alphabetical'"
                                :title="$t('transferListSortAlphabetical')"
                                @click="availableSort = 'alphabetical'"
                            >
                                A-Z
                            </button>
                            <button
                                type="button"
                                class="btn transfer-list-sort-button"
                                :class="{ active: availableSort === 'id' }"
                                :aria-pressed="availableSort === 'id'"
                                :title="$t('transferListSortId')"
                                @click="availableSort = 'id'"
                            >
                                ID
                            </button>
                        </div>
                    </div>
                </div>
                <input
                    type="text"
                    class="form-control form-control-sm mb-2"
                    v-model="searchAvailable"
                    :placeholder="$t(transferListPlaceholder)"
                />
                <div
                    class="border rounded p-2"
                    style="height: 300px; overflow-y: auto"
                >
                    <div
                        v-for="question in filteredAvailable"
                        :key="question.id"
                        class="selectable-item small"
                        :class="{ selected: selectedAvailableIds.includes(question.id) }"
                        @click="toggleSelection(question.id, 'available')"
                    >
                        {{ question.text }}
                        <div class="text-muted small">Id: {{ question.id }}</div>
                    </div>
                </div>
            </div>

            <div class="col-auto d-flex flex-column justify-content-center gap-2 mx-3">
                <button
                    class="btn btn-outline-primary btn-sm table-btn"
                    @click="moveAll('available')"
                >
                    <LucideIcon icon="ChevronsRight" />
                </button>
                <button
                    class="btn btn-outline-primary btn-sm table-btn"
                    @click="moveSelected('available')"
                >
                    <LucideIcon icon="ChevronRight" />
                </button>
                <button
                    class="btn btn-outline-primary btn-sm table-btn"
                    @click="moveSelected('selected')"
                >
                    <LucideIcon icon="ChevronLeft" />
                </button>
                <button
                    class="btn btn-outline-primary btn-sm table-btn"
                    @click="moveAll('selected')"
                >
                    <LucideIcon icon="ChevronsLeft" />
                </button>
            </div>

            <div class="col">
                <h6>{{ $t("common.selectedList") }}</h6>
                <input
                    type="text"
                    class="form-control form-control-sm mb-2"
                    v-model="searchSelected"
                    :placeholder="$t(transferListPlaceholder)"
                />
                <div
                    class="border rounded p-2"
                    style="height: 300px; overflow-y: auto"
                >
                    <template v-if="showItens">
                        <div
                            v-for="question in filteredSelected"
                            :key="question.id"
                            :class="{ selected: selectedSelectedIds.includes(question.id) }"
                            class="selectable-item small"
                            @click="toggleSelection(question.id, 'selected')"
                        >
                            {{ question.text || question.description }}
                            <div class="text-muted small">Id: {{ question.id }}</div>
                        </div>
                    </template>
                    <div
                        v-else
                        class="text-muted small"
                    >
                        {{ $t("quizzes.noItemsSelected") }}
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "TransferListComponent",
        props: {
            available: {
                type: Array,
                required: true,
            },
            modelValue: {
                type: Array,
                required: true,
            },
            transferListTitle: {
                type: String,
                required: false,
                default: "transferListTitle",
            },
            transferListPlaceholder: {
                type: String,
                required: false,
                default: "transferListPlaceholder",
            },
            showSortControl: {
                type: Boolean,
                default: false,
            },
        },
        emits: ["update:modelValue"],
        data() {
            return {
                searchAvailable: "",
                searchSelected: "",
                selectedAvailableIds: [],
                selectedSelectedIds: [],
                availableSort: "id",
            };
        },
        computed: {
            filteredAvailable() {
                const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
                const selectedIds = new Set(selected.map((q) => q?.id));

                const filteredQuestions = this.available
                    .filter((q) => q && !selectedIds.has(q.id))
                    .filter((q) =>
                        (q.text || "").toLowerCase().includes(this.searchAvailable.toLowerCase())
                    );

                return [...filteredQuestions].sort((firstQuestion, secondQuestion) => {
                    if (this.availableSort === "alphabetical") {
                        return new Intl.Collator(this.$i18n.locale, {
                            sensitivity: "base",
                            numeric: true,
                        }).compare(
                            firstQuestion.description || firstQuestion.text || "",
                            secondQuestion.description || secondQuestion.text || ""
                        );
                    }

                    return Number(firstQuestion.id) - Number(secondQuestion.id);
                });
            },
            filteredSelected() {
                const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
                return selected.filter((q) =>
                    (q.text || "").toLowerCase().includes(this.searchSelected.toLowerCase())
                );
            },
            showItens() {
                return this.filteredSelected.length > 0;
            },
        },
        mounted() {
            const initial = Array.isArray(this.modelValue)
                ? this.modelValue.filter((q) => q && q.id !== undefined && q.text !== undefined)
                : [];
            this.selectedSelectedIds = initial.map((q) => q.id);
        },
        methods: {
            toggleSelection(id, list) {
                const selectedIds =
                    list === "available" ? this.selectedAvailableIds : this.selectedSelectedIds;
                const index = selectedIds.indexOf(id);
                if (index === -1) {
                    selectedIds.push(id);
                } else {
                    selectedIds.splice(index, 1);
                }
            },
            moveSelected(from) {
                if (from === "available") {
                    const toMove = this.available.filter((q) =>
                        this.selectedAvailableIds.includes(q.id)
                    );
                    const newSelected = [
                        ...(Array.isArray(this.modelValue) ? this.modelValue : []),
                        ...toMove,
                    ];
                    this.$emit("update:modelValue", newSelected);
                    this.selectedAvailableIds = [];
                } else {
                    const newSelected = (
                        Array.isArray(this.modelValue) ? this.modelValue : []
                    ).filter((q) => !this.selectedSelectedIds.includes(q.id));
                    this.$emit("update:modelValue", newSelected);
                    this.selectedSelectedIds = [];
                }
            },
            moveAll(from) {
                if (from === "available") {
                    const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
                    const availableNotYetSelected = this.available.filter(
                        (q) => !selected.find((sel) => sel.id === q.id)
                    );
                    this.$emit("update:modelValue", [...selected, ...availableNotYetSelected]);
                    this.selectedAvailableIds = [];
                } else {
                    this.$emit("update:modelValue", []);
                    this.selectedSelectedIds = [];
                }
            },
        },
    };
</script>
<style scoped>
    .transfer-list-header {
        display: flex;
        min-height: 32px;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 6px;
    }
    .transfer-list-sort {
        display: flex;
        align-items: center;
        gap: 8px;
    }
    .transfer-list-sort-label {
        color: var(--bs-secondary-color);
        font-size: 0.75rem;
        white-space: nowrap;
    }
    .transfer-list-sort-button {
        width: 32px;
        height: 32px;
        min-width: 32px;
        min-height: 32px;
        padding: 0;
        border-color: var(--color-border-form-control);
        color: var(--bs-body-color);
        font-size: 0.6875rem !important;
        line-height: 1;
        white-space: nowrap;
    }
    .transfer-list-sort-button:hover,
    .transfer-list-sort-button:focus-visible {
        border-color: var(--bs-primary);
        color: var(--bs-primary);
    }
    .transfer-list-sort-button.active {
        border-color: var(--bs-primary);
        background: var(--bs-primary);
        color: var(--bs-white);
    }
    .selectable-item {
        padding: 8px;
        margin-bottom: 4px;
        border-radius: 4px;
        cursor: pointer;
    }
    .selectable-item:hover {
        background-color: var(--color-hover-transfer) !important;
    }
    .selectable-item.selected {
        background-color: var(--color-selected-transfer) !important;
        color: var(--color-body-content) !important;
    }
    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
</style>
