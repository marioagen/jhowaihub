<template>
    <ModalComponent id="typeModal" :isLoading="isLoading" @save="save" ref="ExtractDataModal">
        <template #header>
            <div class="modal-header">
                <h5 class="modal-title">{{ label }}</h5>
                <button class="btn-close" data-bs-dismiss="modal" @click="close" />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <textarea ref="textToCopy" v-model="currentValue" class="form-control" rows="10"
                    @input="handleInput"></textarea>
            </div>
        </template>

        <template #footer>
            <div class="modal-footer">
                <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="close">
                    {{ $t("labelCancel") }}
                </button>
                <button class="btn btn-primary btn-sm" @click="copy">
                    {{ $t("labelCopy") }}
                </button>
                <button class="btn btn-success btn-sm" @click="save" :disabled="!hasChanges">
                    {{ $t("labelSave") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
import ModalComponent from "@/components/global/ModalComponent.vue";
export default {
    components: {
        ModalComponent,
    },
    emits: ['update'],
    props: {
    },
    data: () => ({
        value: "",
        currentValue: "",
        label: "",
        hasChanges: false,
    }),
    methods: {
        open(value, label) {
            this.value = value;
            this.currentValue = value;
            this.label = label;
            this.hasChanges = false;
            this.$refs.ExtractDataModal.open();
        },
        close() {
            this.$refs.ExtractDataModal.close();
        },
        handleInput() {
            this.hasChanges = this.currentValue !== this.value;
        },
        copy() {
            const text = this.$refs.textToCopy.value;
            navigator.clipboard.writeText(text)
        },
        save() {
            if (this.hasChanges) {
                this.$emit('update', this.currentValue);
                this.value = this.currentValue;
                this.hasChanges = false;
                this.close();
            }
        },
    },
};
</script>

<style scoped>
.form-control {
    resize: vertical;
    font-family: inherit;
}
</style>
