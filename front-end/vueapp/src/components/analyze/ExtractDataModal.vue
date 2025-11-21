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
                <span ref="textToCopy">{{value}}</span>
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
        props: {
        },
        data: () => ({
            value: "",
            label: "",
        }),
        methods: {
            open(value, label) {
                console.log("Here 2", value,label);
                this.value = value;
                this.label = label;
                this.$refs.ExtractDataModal.open();
            },
            close() {
                this.$refs.ExtractDataModal.close();
            },
            copy() {
               const text = this.$refs.textToCopy.textContent;
               navigator.clipboard.writeText(text)
            },
        },
    };
</script>
