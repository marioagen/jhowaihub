<template>
    <WorkflowWizard
     :workflowId="routeId"/>
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
            }
        }
    }
</script>