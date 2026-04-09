<template>
    <div
        :class="collapseRootClass"
        :id="collapseId"
        ref="collapseEl"
        :data-bs-parent="parentSelector || undefined"
        :aria-labelledby="labelledBy || undefined"
    >
        <div :class="innerWrapperClass">
            <slot />
        </div>
    </div>
</template>
<script>
    export default {
        name: "CollapseComponent",
        props: {
            collapseId: {
                type: String,
                required: true,
            },
            parentSelector: {
                type: String,
                default: null,
            },
            collapseRootClass: {
                type: String,
                default: "collapse",
            },
            innerWrapperClass: {
                type: String,
                default: "card card-body border-light bg-transparent rounded-3",
            },
            labelledBy: {
                type: String,
                default: null,
            },
        },
        mounted() {
            const options = { toggle: false };
            if (this.parentSelector) {
                options.parent = this.parentSelector;
            }
            this.collapseInstance = new window.bootstrap.Collapse(this.$refs.collapseEl, options);
        },
        beforeUnmount() {
            if (this.collapseInstance) {
                this.collapseInstance.dispose();
            }
        },
        methods: {
            open() {
                this.collapseInstance.show();
            },
            close() {
                this.collapseInstance.hide();
            },
            toggle() {
                this.collapseInstance.toggle();
            },
        },
    };
</script>
