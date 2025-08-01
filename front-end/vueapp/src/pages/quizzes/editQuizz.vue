<template>
    <QuizzesForm 
        :isEdit="true"
        :id="$route.params.id"
    />
</template>

<script>
    import QuizzesForm from '@/components/quizzes/QuizzesForm.vue';
    export default {
        name: "EditQuizz",
        components: {
            QuizzesForm
        },
    }
</script>