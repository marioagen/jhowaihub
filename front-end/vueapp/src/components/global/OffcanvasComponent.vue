<template>
    <div
        :id="id"
        :class="['offcanvas', placementClass]"
        tabindex="-1"
        :aria-labelledby="ariaTitleId"
        ref="offcanvasEl"
    >
        <slot name="header" />
        <div class="offcanvas-body">
            <slot />
        </div>
    </div>
</template>
<script>
    export default {
        name: "OffcanvasComponent",
        props: {
            id: {
                type: String,
                required: true,
            },
            placement: {
                type: String,
                default: "end",
            },
            labelId: {
                type: String,
                default: "",
            },
        },
        computed: {
            placementClass() {
                return `offcanvas-${this.placement}`;
            },
            ariaTitleId() {
                return this.labelId || `${this.id}-label`;
            },
        },
        mounted() {
            this.offcanvasInstance = new window.bootstrap.Offcanvas(this.$refs.offcanvasEl);
        },
        beforeUnmount() {
            this.offcanvasInstance?.dispose?.();
            this.offcanvasInstance = null;
        },
        methods: {
            open() {
                this.offcanvasInstance?.show();
            },
            close() {
                this.offcanvasInstance?.hide();
            },
        },
    };
</script>
