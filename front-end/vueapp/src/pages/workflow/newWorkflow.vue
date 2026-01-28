<template>
    <WorkflowWizard ref="wizard" :workflowId="routeId" />
</template>

<script>
    import WorkflowWizard from "@/components/workflow/wizard/WorkflowWizard.vue";
    export default {
        name: "NewWorkflow",
        components: {
            WorkflowWizard,
        },
        computed: {
            routeId() {
                const id = this.$route.params.id;
                return id ? parseInt(id) : undefined;
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