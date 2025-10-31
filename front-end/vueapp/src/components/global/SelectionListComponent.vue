<template>
    <div class="mt-3">
        <label class="form-label fw-semibold mb-0">{{ $t(labelPanel) }}</label>
        <div class="card">
            <div class="card-header p-2">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="fw-semibold">{{ $t(labelSelectedQuantity) }} ({{ selected.length }})</span>
                    <div class="float-end">
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm me-2 fw-semibold"
                            @click="selectAll"
                        >
                            <LucideIcon icon="CheckCheck" :size="15" />
                            {{ $t("labelSelectAll") }}
                        </button>
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm fw-semibold"
                            @click="clearSelection"
                        >
                            <LucideIcon icon="CircleX" :size="15" />
                            {{ $t("labelClearSelection") }}
                        </button>
                    </div>
                </div>
                <div class="mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><i class="fas fa-search text-secondary"></i></span>
                        <input
                            type="text"
                            class="form-control form-control-sm"
                            :placeholder="$t(labelSearch)"
                            v-model="search"
                        />
                    </div>
                </div>
                <div class="selection-list">
                    <div v-if="loading" class="text-center">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">{{ $t("labelLoading") }}</span>
                        </div>
                    </div>
                    <div v-if="!loading" v-for="item in filteredItems" :key="item.id">
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
                                        <div class="text-muted small">{{ item.email }}</div>
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
                    this.$emit("update:selectedItems", val);
                },
            },
            filteredItems() {
                if (!this.search) return this.items;
                return this.items.filter((item) => item.name.toLowerCase().includes(this.search.toLowerCase()));
            },
        },
        methods: {
            selectAll() {
                this.selected = this.filteredItems.map((item) => item.id);
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
        max-height: 80px;
        min-height: 80px;
        overflow-y: auto;
    }

    .initials {
        width: 30px;
        height: 30px;
    }
</style>
