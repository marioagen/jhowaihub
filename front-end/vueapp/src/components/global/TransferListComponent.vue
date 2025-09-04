<template>
    <div class="container mt-1">
        <div class="row">
            <div class="col">
                <h6>{{ $t(transferListTitle) }}</h6>
                <input
                    type="text"
                    class="form-control form-control-sm mb-2"
                    v-model="searchAvailable"
                    :placeholder="$t(transferListPlaceholder)"
                />
                <div class="border rounded p-2" style="height: 300px; overflow-y: auto;">
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
                <h6>{{ $t("labelSelectedList") }}</h6>
                <input
                    type="text"
                    class="form-control form-control-sm mb-2"
                    v-model="searchSelected"
                    :placeholder="$t(transferListPlaceholder)"
                />
                <div class="border rounded p-2" style="height: 300px; overflow-y: auto;">
                    <div
                        v-if="showItens"
                        v-for="question in filteredSelected"
                        :key="question.id"
                        :class="{ selected: selectedSelectedIds.includes(question.id) }"
                        class="selectable-item small"
                        @click="toggleSelection(question.id, 'selected')"
                    >
                        {{ question.text || question.description }}
                        <div class="text-muted small">Id: {{ question.id }} </div>
                    </div>
                    <div v-else class="text-muted small">Nenhum item selecionado</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'TransferListComponent',
    props: {
        available: {
            type: Array,
            required: true
        },
        modelValue: {
            type: Array,
            required: true
        },
        transferListTitle: {
            type: String,
            required: false,
            default: "transferListTitle"
        },
        transferListPlaceholder: {
            type: String,
            required: false,
            default: "transferListPlaceholder"
        },
    },
    emits: ['update:modelValue'],
    data() {
        return {
            searchAvailable: '',
            searchSelected: '',
            selectedAvailableIds: [],
            selectedSelectedIds: []
        };
    },
    computed: {
        filteredAvailable() {
            const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
            const selectedIds = new Set(selected.map(q => q?.id));

            return this.available
                .filter(q => q && !selectedIds.has(q.id))
                .filter(q => (q.text || '').toLowerCase().includes(this.searchAvailable.toLowerCase()));
        },
        filteredSelected() {
            const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
            return selected.filter(q =>
                (q.text || '').toLowerCase().includes(this.searchSelected.toLowerCase())
            );
        },
        showItens() {
            return this.filteredSelected.length > 0;
        }
    },
    mounted() {
        const initial = Array.isArray(this.modelValue)
            ? this.modelValue.filter(q => q && q.id !== undefined && q.text !== undefined)
            : [];
        this.selectedSelectedIds = initial.map(q => q.id);
    },
    methods: {
        toggleSelection(id, list) {
            const selectedIds = list === 'available' ? this.selectedAvailableIds : this.selectedSelectedIds;
            const index = selectedIds.indexOf(id);
            if (index === -1) {
                selectedIds.push(id);
            } else {
                selectedIds.splice(index, 1);
            }
        },
        moveSelected(from) {
            if (from === 'available') {
                const toMove = this.available.filter(q => this.selectedAvailableIds.includes(q.id));
                const newSelected = [...(Array.isArray(this.modelValue) ? this.modelValue : []), ...toMove];
                this.$emit('update:modelValue', newSelected);
                this.selectedAvailableIds = [];
            } else {
                const newSelected = (Array.isArray(this.modelValue) ? this.modelValue : [])
                    .filter(q => !this.selectedSelectedIds.includes(q.id));
                this.$emit('update:modelValue', newSelected);
                this.selectedSelectedIds = [];
            }
        },
        moveAll(from) {
            if (from === 'available') {
                const selected = Array.isArray(this.modelValue) ? this.modelValue : [];
                const availableNotYetSelected = this.available.filter(
                    q => !selected.find(sel => sel.id === q.id)
                );
                this.$emit('update:modelValue', [...selected, ...availableNotYetSelected]);
                this.selectedAvailableIds = [];
            } else {
                this.$emit('update:modelValue', []);
                this.selectedSelectedIds = [];
            }
        }
    }
};
</script>

<style scoped>
.selectable-item {
    padding: 8px;
    margin-bottom: 4px;
    border-radius: 4px;
    cursor: pointer;
}
.selectable-item:hover {
    background-color: #f0f0f0;
}
.selectable-item.selected {
    background-color: #0d6efd;
    color: white;
}
</style>
