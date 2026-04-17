<template>
    <input
        ref="inputEl"
        type="checkbox"
        class="form-check-input m-0"
        :disabled="visibleCardIds.length === 0"
        :aria-label="$t('workflow.bulk.selectStepRows')"
        @change="onChange"
    />
</template>
<script>
    export default {
        name: "AccordionStepHeaderCheckbox",
        props: {
            visibleCardIds: {
                type: Array,
                required: true,
            },
            selectedCardIds: {
                type: Array,
                required: true,
            },
        },
        emits: ["toggle-step"],
        watch: {
            visibleCardIds: {
                deep: true,
                handler() {
                    this.syncDom();
                },
            },
            selectedCardIds: {
                deep: true,
                handler() {
                    this.syncDom();
                },
            },
        },
        mounted() {
            this.syncDom();
        },
        updated() {
            this.syncDom();
        },
        methods: {
            allSelected() {
                const ids = this.visibleCardIds;
                if (!ids.length) return false;
                return ids.every((id) => this.selectedCardIds.includes(id));
            },
            someSelected() {
                const ids = this.visibleCardIds;
                return ids.some((id) => this.selectedCardIds.includes(id));
            },
            syncDom() {
                const el = this.$refs.inputEl;
                if (!el) return;
                if (this.visibleCardIds.length === 0) {
                    el.checked = false;
                    el.indeterminate = false;
                    return;
                }
                const all = this.allSelected();
                const some = this.someSelected();
                el.checked = all && this.visibleCardIds.length > 0;
                el.indeterminate = some && !all;
            },
            onChange(e) {
                const checked = e.target.checked;
                this.$emit("toggle-step", {
                    cardIds: [...this.visibleCardIds],
                    selectAll: checked,
                });
                this.$nextTick(() => this.syncDom());
            },
        },
    };
</script>
