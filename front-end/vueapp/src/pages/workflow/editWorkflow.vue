<template>
    <WorkflowBoards 
        :isEdit="true"
        :id="$route.params.id"
    />
</template>

<script>
    import WorkflowBoards from "@/components/workflow/WorkflowBoards.vue";
    export default {
        name: "EditWorkflow",
        components: {
            WorkflowBoards,
        },
    }
</script>