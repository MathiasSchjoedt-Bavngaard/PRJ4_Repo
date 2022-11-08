import { useMutation, useQueryClient } from "react-query";
import axios, { AxiosResponse } from "axios";
import { request } from "../utils/axios";

import type { ExerciseIds } from "../../interfaces/Exercise";
// workoutId: number,
//   exerciseIds: ExerciseIds
export const addExercisesToWorkout = async (data) => {
  console.log(data.workoutId, data.exerciseIds);
  return request({
    url: `${data.workoutId}/AddExercise/list`,
    method: "post",
    data: data.exerciseIds,
  });
};

export const useAddExercisesToWorkoutData = () => {
  const queryClient = useQueryClient();
  return useMutation(addExercisesToWorkout, {
    onMutate: async (newExerciseList) => {
      await queryClient.cancelQueries("workoutsKey");
      const previouesWorkoutData = queryClient.getQueryData("workoutsKey");
      queryClient.setQueryData("workoutsKey", (oldQueryData) => {
        return {
          ...oldQueryData,
          data: [
            ...oldQueryData.data,
            { ...(oldQueryData?.data?.length + 1), ...newExerciseList },
          ],
        };
      });
      return {
        previouesWorkoutData,
      };
    },
    onError: (_error, _workout, context) => {
      queryClient.setQueryData("workoutsKey", context.previouesWorkoutData);
      alert("there was an error");
    },
    onSettled: () => {
      queryClient.invalidateQueries("workoutsKey");
      queryClient.invalidateQueries("workoutKey");
    },
  });
};
