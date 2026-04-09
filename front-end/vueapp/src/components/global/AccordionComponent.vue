<template>
    <div
        :id="accordionId"
        :class="['accordion', { 'accordion-flush': flush }]"
    >
        <slot />
    </div>
</template>
<script>
    import { defineComponent, h } from "vue";
    import CollapseComponent from "./CollapseComponent.vue";

    export const AccordionItem = defineComponent({
        name: "AccordionItem",
        components: { CollapseComponent },
        inject: {
            accordionParentSelector: {
                from: "accordionParentSelector",
                default: null,
            },
        },
        props: {
            itemId: {
                type: String,
                required: true,
            },
            visible: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                isExpanded: this.visible,
            };
        },
        computed: {
            headingId() {
                return `heading-${this.itemId}`;
            },
        },
        mounted() {
            this.$nextTick(() => {
                const collapse = this.$refs.collapse;
                const el = collapse?.$refs?.collapseEl;
                if (el) {
                    el.addEventListener("shown.bs.collapse", this.onShown);
                    el.addEventListener("hidden.bs.collapse", this.onHidden);
                }
                if (this.visible && collapse) {
                    collapse.open();
                }
            });
        },
        beforeUnmount() {
            const collapse = this.$refs.collapse;
            const el = collapse?.$refs?.collapseEl;
            if (el) {
                el.removeEventListener("shown.bs.collapse", this.onShown);
                el.removeEventListener("hidden.bs.collapse", this.onHidden);
            }
        },
        methods: {
            onHeaderClick() {
                this.$refs.collapse?.toggle();
            },
            onShown() {
                this.isExpanded = true;
            },
            onHidden() {
                this.isExpanded = false;
            },
            open() {
                this.$refs.collapse?.open();
            },
            close() {
                this.$refs.collapse?.close();
            },
            toggle() {
                this.$refs.collapse?.toggle();
            },
        },
        render() {
            return h("div", { class: "accordion-item" }, [
                h("h2", { class: "accordion-header", id: this.headingId }, [
                    h(
                        "button",
                        {
                            type: "button",
                            class: ["accordion-button", { collapsed: !this.isExpanded }],
                            "aria-expanded": this.isExpanded ? "true" : "false",
                            "aria-controls": this.itemId,
                            onClick: (e) => {
                                e.preventDefault();
                                this.onHeaderClick();
                            },
                        },
                        this.$slots.header?.() ?? []
                    ),
                ]),
                h(
                    CollapseComponent,
                    {
                        ref: "collapse",
                        collapseId: this.itemId,
                        parentSelector: this.accordionParentSelector,
                        collapseRootClass: "accordion-collapse collapse",
                        innerWrapperClass: "accordion-body",
                        labelledBy: this.headingId,
                    },
                    { default: () => this.$slots.default?.() ?? [] }
                ),
            ]);
        },
    });

    export default {
        name: "AccordionComponent",
        props: {
            accordionId: {
                type: String,
                required: true,
            },
            flush: {
                type: Boolean,
                default: false,
            },
        },
        provide() {
            return {
                accordionParentSelector: `#${this.accordionId}`,
            };
        },
    };
</script>
