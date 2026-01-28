<template>
    <WorkflowWizard
        ref="wizard"
        :isEdit="true"
        :workflowId="routeId"
    />
</template>

<script>
    import WorkflowWizard from "@/components/workflow/wizard/WorkflowWizard.vue";
    export default {
        name: "EditWorkflow",
        components: {
            WorkflowWizard,
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
        beforeRouteLeave(to, from, next) {
            const wizard = this.$refs.wizard;
            if (wizard && !wizard.canLeave) {
                wizard.checkNavigation(() => {
                    wizard.canLeave = true;
                    this.$router.push(to);
                });
                next(false);
            } else {
                next();
            }
        },
    };
</script>