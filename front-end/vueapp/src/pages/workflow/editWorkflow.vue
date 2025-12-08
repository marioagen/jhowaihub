<template>
    <WorkflowWizard
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
    }
</script>