<template>
    <WorkflowForm
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import WorkflowForm from "@/components/workflow/form/WorkflowForm.vue";
    export default {
        name: "EditWorkflow",
        components: {
            WorkflowForm,
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>