<template>
    <div :class="{ 'mt-3': !compact }">
        <label
            v-if="showLabel"
            class="form-label fw-semibold mb-0"
        >
            {{ $t(labelPanel) }}
        </label>
        <div class="card">
            <div class="card-header p-2">
                <div
                    v-if="!hideBulkToolbar"
                    class="d-flex justify-content-between align-items-center mb-2"
                >
                    <span class="fw-semibold">
                        {{ $t(labelSelectedQuantity) }} ({{ selected.length }})
                    </span>
                    <div class="float-end">
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm me-2 fw-semibold"
                            @click="selectAll"
                        >
                            <LucideIcon
                                icon="CheckCheck"
                                :size="15"
                            />
                            {{ $t("common.selectAll") }}
                        </button>
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm fw-semibold"
                            @click="clearSelection"
                        >
                            <LucideIcon
                                icon="CircleX"
                                :size="15"
                            />
                            {{ $t("common.clearSelection") }}
                        </button>
                    </div>
                </div>
                <div
                    v-if="selected.length > 0"
                    class="mb-3 px-1"
                >
                    <label class="form-label small fw-semibold mb-2 d-block">
                        {{ $t(selectionChipLabelKey) }}
                    </label>
                    <div class="d-flex flex-wrap gap-2">
                        <div
                            v-for="itemId in selecteOrderedByName"
                            :key="itemId"
                            class="badge rounded-pill d-flex align-items-center px-2 py-1 selected-item-chip"
                        >
                            <LucideIcon
                                :icon="resolvedChipIcon"
                                class="me-1 chip-leading-icon"
                                :size="14"
                            />
                            <span class="me-1">
                                {{ getItemName(itemId) }}
                            </span>
                            <button
                                type="button"
                                class="chip-remove-btn"
                                :title="$t('common.removeSelectionChip')"
                                :aria-label="$t('common.removeSelectionChip')"
                                @click.stop="removeItemFromSelection(itemId)"
                            >
                                <LucideIcon
                                    icon="X"
                                    :size="14"
                                />
                            </button>
                        </div>
                    </div>
                </div>
                <div class="mb-3">
                    <div class="input-group">
                        <span class="input-group-text border-end-0">
                            <i class="fas fa-search text-secondary"></i>
                        </span>
                        <input
                            type="text"
                            class="form-control form-control-sm"
                            :placeholder="$t(labelSearch)"
                            v-model="search"
                        />
                    </div>
                </div>
                <div
                    class="selection-list"
                    :style="{ maxHeight: listHeight, minHeight: listHeight }"
                >
                    <div
                        v-if="loading"
                        class="text-center"
                    >
                        <div
                            class="spinner-border text-primary"
                            role="status"
                        >
                            <span class="visually-hidden">{{ $t("common.loading") }}</span>
                        </div>
                    </div>
                    <div
                        v-if="!loading"
                        v-for="item in filteredItems"
                        :key="item.id"
                    >
                        <div class="form-check d-flex align-items-center">
                            <input
                                class="form-check-input me-3"
                                type="checkbox"
                                :id="`${id}-${item.id}`"
                                :value="item.id"
                                v-model="selected"
                            />
                            <div v-if="type === 'user-list'">
                                <label
                                    :for="`${id}-${item.id}`"
                                    class="form-check-label d-flex align-items-center w-100"
                                >
                                    <div
                                        class="rounded-circle d-flex align-items-center justify-content-center btn-primary fw-bold me-3 initials"
                                    >
                                        {{ getInitials(item.name) }}
                                    </div>
                                    <div>
                                        <div class="fw-semibold">{{ item.name }}</div>
                                        <div class="text-muted small">
                                            {{ userSecondaryLine(item) }}
                                        </div>
                                    </div>
                                </label>
                            </div>
                            <div v-else>
                                <label
                                    :for="`${id}-${item.id}`"
                                    class="form-check-label d-flex align-items-center w-100"
                                >
                                    <div class="fw-semibold">{{ item.name }}</div>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                <slot name="footer"></slot>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "SelectionListComponent",
        props: {
            id: {
                type: String,
                required: true,
            },
            items: {
                type: Array,
                required: true,
            },
            loading: {
                type: Boolean,
                default: false,
            },
            selectedItems: {
                type: Array,
                default: () => [],
            },
            labelPanel: {
                type: String,
                required: true,
            },
            labelSelectedQuantity: {
                type: String,
                required: true,
            },
            labelSearch: {
                type: String,
                required: true,
            },
            type: {
                type: String,
                default: "simple-list",
            },
            listHeight: {
                type: String,
                default: "80px",
            },
            chipIcon: {
                type: String,
                default: "",
            },
            maxSelections: {
                type: Number,
                default: null,
            },
            hideBulkToolbar: {
                type: Boolean,
                default: false,
            },
            compact: {
                type: Boolean,
                default: false,
            },
            selectionChipLabelKey: {
                type: String,
                default: "common.selectionList",
            },
            showLabel: {
                type: Boolean,
                default: true,
            },
        },
        emits: ["update:selectedItems"],
        data() {
            return {
                search: "",
            };
        },
        computed: {
            selected: {
                get() {
                    return this.selectedItems;
                },
                set(val) {
                    let next = val;
                    if (this.maxSelections === 1 && Array.isArray(val) && val.length > 1) {
                        next = [val[val.length - 1]];
                    }
                    this.$emit("update:selectedItems", next);
                },
            },
            filteredItems() {
                if (!this.search) return this.items;
                return this.items.filter((item) =>
                    (item.name || "").toLowerCase().includes(this.search.toLowerCase())
                );
            },
            resolvedChipIcon() {
                if (this.chipIcon) {
                    return this.chipIcon;
                }
                return this.type === "user-list" ? "User" : "UsersRound";
            },
            selecteOrderedByName() {
                return [...this.selected].sort((a, b) => {
                    const nameA = this.getItemName(a).toLowerCase();
                    const nameB = this.getItemName(b).toLowerCase();
                    return nameA.localeCompare(nameB);
                });
            },
        },
        methods: {
            getItemName(id) {
                const item = this.items.find((i) => i.id === id);
                return item?.name ?? "—";
            },
            removeItemFromSelection(id) {
                this.selected = this.selectedItems.filter((itemId) => itemId !== id);
            },
            selectAll() {
                if (this.maxSelections === 1 && this.filteredItems.length > 0) {
                    this.selected = [this.filteredItems[0].id];
                    return;
                }
                this.selected = this.filteredItems.map((item) => item.id);
            },
            userSecondaryLine(item) {
                if (item.profile?.name) return item.profile.name;
                if (item.role) return item.role;
                return item.email ?? "";
            },
            clearSelection() {
                this.selected = [];
            },
            getInitials(name) {
                if (!name) return "";
                const parts = name.trim().split(" ");
                if (parts.length === 1) {
                    const n = parts[0];
                    return (n[0] || "").toUpperCase() + (n[n.length - 1] || "").toUpperCase();
                }
                const first = parts[0][0] || "";
                const last = parts[parts.length - 1].slice(-1) || "";
                return (first + last).toUpperCase();
            },
        },
    };
</script>
<style scoped>
    .selection-list {
        overflow-y: auto;
    }

    .initials {
        width: 30px;
        height: 30px;
    }

    .selected-item-chip {
        background-color: #155dfc !important;
        color: white !important;
    }

    .chip-leading-icon {
        flex-shrink: 0;
    }

    .chip-remove-btn {
        border: none;
        background: transparent;
        color: inherit;
        padding: 0 0 0 4px;
        margin: 0;
        line-height: 1;
        display: inline-flex;
        align-items: center;
        cursor: pointer;
        opacity: 0.85;
    }

    .chip-remove-btn:hover,
    .chip-remove-btn:focus-visible {
        opacity: 1;
    }

    .chip-remove-btn:focus-visible {
        outline: 2px solid rgba(255, 255, 255, 0.8);
        outline-offset: 1px;
        border-radius: 2px;
    }
</style>
